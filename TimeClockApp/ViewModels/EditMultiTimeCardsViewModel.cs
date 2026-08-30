using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class EditMultiTimeCardsViewModel(EditTimeCardService service) : BaseViewModel, IQueryAttributable
	{
		protected readonly EditTimeCardService cardService = service;

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
			Loading = true;
			HasError = false;

			try
			{
				PickerMinDate = cardService.GetAppFirstRunDate();

				List<Project> pro = await cardService.GetProjectsListAsync();
				ProjectList = pro.ToObservableCollection();
				List<Phase> ph = await cardService.GetPhaseListAsync();
				PhaseList = ph.ToObservableCollection();

				List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
				TimeCards = t.ToObservableCollection();

				// Needed to clear the selection when navigating back to this page, otherwise the previous selection will remain selected and cause issues with the UI.
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
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "EditTimeCardViewModel.OnAppearing");
			}
			finally
			{
				Loading = false;
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
			Loading = true;
			HasError = false;
			try
			{
				if (SelectedTimeCards.Count > 0)
				{
					if (CheckEndTime && CheckStartTime && await ValidateTimeFixer(TimeCard_StartTime, TimeCard_EndTime))
					{
						await App.AlertSvc!.ShowAlertAsync("ERROR", "End time is before start time. Please correct the times.").ConfigureAwait(false);
						return;
					}
					List<TimeCard> cards = SelectedTimeCards.OfType<TimeCard>().ToList();
					if (CheckStartTime)
					{
						if (TimeCard_StartTime > TimeOnly.FromDateTime(DateTime.Now))
						{
							await App.AlertSvc!.ShowAlertAsync("ERROR", "Start time cannot be in the future. Please correct the times.").ConfigureAwait(false);
							return;
						}
						foreach (TimeCard tt in cards)
						{
							if (!CheckEndTime && tt.TimeCard_EndTime < TimeCard_StartTime && !tt.TimeCard_bReadOnly)
							{
								await App.AlertSvc!.ShowAlertAsync("ERROR", $"TimeCardId: {tt.TimeCardId} - End time will be before start time. Please correct the times.").ConfigureAwait(false);
								return;
							}
							else if (CheckEndTime && TimeCard_EndTime < TimeCard_StartTime && !tt.TimeCard_bReadOnly)
							{
								await App.AlertSvc!.ShowAlertAsync("ERROR", $"TimeCardId: {tt.TimeCardId} - End time will be before start time. Please correct the times.").ConfigureAwait(false);
								return;
							}
						}
					}
					if (CheckEndTime)
					{
						if (TimeCard_EndTime > TimeOnly.FromDateTime(DateTime.Now))
						{
							TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(TimeOnly.FromDateTime(DateTime.Now).Hour, TimeOnly.FromDateTime(DateTime.Now).Minute));
							await AppShell.DisplayToastAsync("End time cannot be in the future. Auto-corrected the end time.", true).ConfigureAwait(false);
						}
					}
					foreach (TimeCard tc in cards)
					{
						if ((tc.TimeCard_Status == ShiftStatus.ClockedIn || tc.TimeCard_Status == ShiftStatus.ClockedOut) && !tc.TimeCard_bReadOnly)
						{
							if (CheckStartTime)
							{
								tc.TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(TimeCard_StartTime.Hour, TimeCard_StartTime.Minute));
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
						}

						if (CheckEndTime && tc.TimeCard_Status == ShiftStatus.ClockedOut && !tc.TimeCard_bReadOnly)
						{
							tc.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(TimeCard_EndTime.Hour, TimeCard_EndTime.Minute));
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
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "EditMultiTimeCardsViewModel.OnSaveCommand");
				await AppShell.DisplayToastAsync("Failed to update time cards.", true);
			}
			finally
			{
				Loading = false;
			}
		}

		/// <summary>
		/// Validates the time range and if the end time is before the start time, it will auto-fix it.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		private async Task<bool> ValidateTimeFixer(TimeOnly start, TimeOnly end)
		{
			// If the end time is after the start time and the end time is after 12:00 PM, auto-fix it by adding 12 hours to the start time.
			if (end < start && end >= new TimeOnly(12, 00, 00))
			{
				TimeCard_StartTime = start.AddHours(12);
				await AppShell.DisplayToastAsync("Auto corrected StartTime.", true).ConfigureAwait(false);
			}
			return (TimeCard_EndTime < TimeCard_StartTime);
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