using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class EditMultiTimeCardsViewModel(EditTimeCardService service, IProjectListService projectListService) : BaseViewModel, IQueryAttributable
	{
		protected readonly EditTimeCardService cardService = service;
		private readonly IProjectListService _projectListService = projectListService;

		[ObservableProperty]
		public partial ObservableCollection<TimeCard> TimeCards { get; set; } = new();
		[ObservableProperty]
		public partial ObservableCollection<Object> SelectedTimeCards { get; set; } = new();

		// Used to pre-select a time card when navigating to this page from another page.
		private int PreSelectedTimeCardID { get; set; } = 0;

		[ObservableProperty]
		public partial DateOnly TimeCard_Date { get; set; }
		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(StartTime))]
		public partial TimeOnly TimeCard_StartTime { get; set; } = new(0);
		public TimeOnly StartTime
		{
			get => TimeCard_StartTime;
			set => TimeCard_StartTime = TimeHelper.RoundTimeOnly(value);
		}

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(EndTime))]
		public partial TimeOnly TimeCard_EndTime { get; set; } = new(0);
		public TimeOnly EndTime
		{
			get => TimeCard_EndTime;
			set => TimeCard_EndTime = TimeHelper.RoundTimeOnly(value);
		}

#region "DatePicker Min/Max Bindings"
		public DateTime PickerMinDate { get; set; }
		public DateTime PickerMaxDate { get; } = DateTime.Now;
#endregion

		[ObservableProperty]
		public partial ObservableCollection<Project> ProjectList { get; set; } = new();
		[ObservableProperty]
		public partial Project? SelectedProject { get; set; } = null;
		[ObservableProperty]
		public partial ObservableCollection<Phase> PhaseList { get; set; } = new();
		[ObservableProperty]
		public partial Phase? SelectedPhase { get; set; } = null;

		[ObservableProperty]
		public partial bool CheckProjectPhase { get; set; }
		[ObservableProperty]
		public partial bool CheckDate { get; set; }
		[ObservableProperty]
		public partial bool CheckStartTime { get; set; } = true;
		[ObservableProperty]
		public partial bool CheckEndTime { get; set; }

		public void ApplyQueryAttributes(IDictionary<string, object> query)
		{
			if (query.ContainsKey("id") && int.TryParse(query["id"].ToString(), out int t))
			{ PreSelectedTimeCardID = t; }
		}

		[RelayCommand]
		public async Task OnAppearing()
		{
			HasError = false;

			try
			{
				PickerMinDate = cardService.GetAppFirstRunDate();

				List<Project> pro = await _projectListService.GetProjectsListAsync();
				ProjectList = pro.ToObservableCollection();
				List<Phase> ph = await _projectListService.GetPhaseListAsync();
				PhaseList = ph.ToObservableCollection();

				List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
				TimeCards = t.ToObservableCollection();

				// Needed to clear the selection when navigating back to this page, otherwise the previous selection will remain selected and cause issues with the UI.
				if (SelectedTimeCards.Count > 0)
					SelectedTimeCards.Clear();

				if (PreSelectedTimeCardID > 0)
				{
					TimeCard? preSelected = TimeCards.FirstOrDefault(tc => tc.TimeCardId == PreSelectedTimeCardID);
					if (preSelected != null)
					{
						SelectedTimeCards.Add(preSelected);

						TimeCard_Date = preSelected.TimeCard_Date;
						TimeCard_StartTime = preSelected.TimeCard_StartTime;
						TimeCard_EndTime = preSelected.TimeCard_EndTime;
						SelectedProject = preSelected.Project;
						SelectedPhase = preSelected.Phase;
					}
				}
				else
				{
					SelectedProject = cardService.GetCurrentProjectEntity();
					SelectedPhase = cardService.GetCurrentPhaseEntity();
					TimeCard_Date = TimeCards.FirstOrDefault()?.TimeCard_Date ?? DateOnly.FromDateTime(DateTime.Now);
					TimeCard_StartTime = TimeCards.FirstOrDefault()?.TimeCard_StartTime ?? new TimeOnly(0);
					TimeCard_EndTime = TimeCards.FirstOrDefault()?.TimeCard_EndTime ?? new TimeOnly(0);
				}
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "EditMultiTimeCardViewModel.OnAppearing");
			}
		}

		[RelayCommand]
		private async Task OnSelectionChanged()
		{
			if (PreSelectedTimeCardID == 0 && SelectedTimeCards.Count == 1)
			{
				TimeCard t = (TimeCard)SelectedTimeCards[0];
				if ((t.TimeCard_Status == ShiftStatus.ClockedIn || t.TimeCard_Status == ShiftStatus.ClockedOut) && !t.TimeCard_bReadOnly)
				{
					if (CheckStartTime)
					{
						TimeCard_StartTime = t.TimeCard_StartTime;
					}
					if (CheckDate)
					{
						TimeCard_Date = t.TimeCard_Date;
					}
					if (CheckProjectPhase)
					{
						SelectedProject = t.Project;
						SelectedPhase = t.Phase;
					}
				}
				if (CheckEndTime && t.TimeCard_Status == ShiftStatus.ClockedOut && !t.TimeCard_bReadOnly)
				{
					TimeCard_EndTime = t.TimeCard_EndTime;
				}
			}
		}

		[RelayCommand]
		private async Task OnSave()
		{
			if (SelectedTimeCards.Count == 0)
			{
				await AppShell.DisplayToastAsync("No time cards selected.", true).ConfigureAwait(false);
				return;
			}
			HasError = false;
			try
			{
				if (SelectedTimeCards.Count > 0)
				{
					if (CheckEndTime && CheckStartTime)
					{
						if (!ValidateTimeFixer(TimeCard_StartTime, TimeCard_EndTime))
						{
							await App.AlertSvc!.ShowAlertAsync("ERROR", "End time is before start time. Please correct the times.").ConfigureAwait(false);
							return;
						}
					}

					List<TimeCard> cards = SelectedTimeCards.OfType<TimeCard>().ToList();
					List<TimeCard> cardsToRemove = [];
					// Remove any time cards that are read-only or deleted, or have invalid times based on the selected start and end times.
					foreach (TimeCard tt in cards)
					{
						if (tt.TimeCard_bReadOnly || tt.TimeCard_Status == ShiftStatus.Deleted)
						{
							cardsToRemove.Add(tt);
						}
						if (tt.TimeCard_Status == ShiftStatus.ClockedOut && !CheckEndTime && CheckStartTime && tt.TimeCard_EndTime < TimeCard_StartTime)
						{
							cardsToRemove.Add(tt);
						}
						else if (tt.TimeCard_Status == ShiftStatus.ClockedOut && CheckEndTime && !CheckStartTime && TimeCard_EndTime < tt.TimeCard_StartTime)
						{
							cardsToRemove.Add(tt);
						}
						else if (tt.TimeCard_Status == ShiftStatus.ClockedIn && CheckStartTime && tt.TimeCard_EndTime < TimeCard_StartTime)
						{
							cardsToRemove.Add(tt);
						}
					}
					// do the removal from SelectedTimeCards
					if (cardsToRemove.Count > 0)
					{
						for (int j = 0; j < cardsToRemove.Count; j++)
						{
							cards.Remove(cardsToRemove[j]);
							//update the UI
							SelectedTimeCards.Remove(cardsToRemove[j]);
						}
						MainThread.BeginInvokeOnMainThread(async () =>
						{
							await App.AlertSvc!.ShowAlertAsync("NOTICE", "One or more Time Cards have invalid times and was removed from selection. Check if the End time is before start time and correct the times.").ConfigureAwait(false);
						});
					}

					if (cards.Count == 0)
					{
						await App.AlertSvc!.ShowAlertAsync("NOTICE", "No valid time cards to update. Check if the End time is before start time and correct the times.").ConfigureAwait(false);
						return;
					}
					else
					{
						// update the valid time cards
						foreach (TimeCard tc in cards)
						{
							if (!tc.TimeCard_bReadOnly && tc.TimeCard_Status != ShiftStatus.Deleted)
							{
								// used to flag if the end time was updated, so we don't update it again if the start time was also updated.
								bool bEndTimeDone = false;
								if (tc.TimeCard_Status == ShiftStatus.ClockedIn || tc.TimeCard_Status == ShiftStatus.ClockedOut)
								{
									if (CheckStartTime)
									{
										if (CheckEndTime && tc.TimeCard_Status == ShiftStatus.ClockedOut)
										{
											if (TimeCard_EndTime > TimeCard_StartTime)
											{
												tc.TimeCard_StartTime = TimeCard_StartTime;
												tc.TimeCard_EndTime = TimeCard_EndTime;
												bEndTimeDone = true;
											}
										}
										else if ((tc.TimeCard_EndTime == new TimeOnly(0) && tc.TimeCard_Status == ShiftStatus.ClockedIn) || tc.TimeCard_EndTime > TimeCard_StartTime)
										{
											tc.TimeCard_StartTime = TimeCard_StartTime;
										}
									}
									if (CheckDate)
									{
										tc.TimeCard_Date = TimeCard_Date;
									}
									if (CheckProjectPhase)
									{
										tc.Project = SelectedProject;
										tc.ProjectId = SelectedProject?.ProjectId ?? 1;
										tc.ProjectName = SelectedProject?.Name ?? ".None";
										tc.Phase = SelectedPhase;
										tc.PhaseId = SelectedPhase?.PhaseId ?? 1;
										tc.PhaseTitle = SelectedPhase?.PhaseTitle ?? ".Misc";
									}

									if (!bEndTimeDone && CheckEndTime && tc.TimeCard_Status == ShiftStatus.ClockedOut)
									{
										if (tc.TimeCard_StartTime < TimeCard_EndTime)
										{
											tc.TimeCard_EndTime = TimeCard_EndTime;
										}
									}
								}
							}
						}
						if (await cardService.UpdateTimeCardListAsync(cards))
						{
							await RefreshTimeCardsAsync();
							await AppShell.DisplayToastAsync("Time cards updated successfully.").ConfigureAwait(false);
						}
						else
						{
							HasError = true;
							Log.WriteLine($"Failed to update time cards.", "EditMultiTimeCardsViewModel.OnSaveCommand");
						}
					}
				}
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "EditMultiTimeCardsViewModel.OnSaveCommand");
				await AppShell.DisplayToastAsync("Failed to update time cards.", true);
			}
		}

		private bool ValidateTimeFixer(TimeOnly start, TimeOnly end)
		{
			return (end >= start && end <= TimeOnly.FromDateTime(DateTime.Now) && start <= TimeOnly.FromDateTime(DateTime.Now));
		}

		[RelayCommand]
		private async Task RefreshTimeCardsAsync()
		{
			List<TimeCard> timeCards = await cardService.GetLastTimeCardForAllEmployeesAsync();
			TimeCards = timeCards.ToObservableCollection();
		}

		[RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}
	}
}