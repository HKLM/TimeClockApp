#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class TimeCardManagerViewModel(EditTimeCardService cardService) : BaseViewModel
	{
		private readonly EditTimeCardService cardService = cardService;

		[ObservableProperty]
		public partial ObservableCollection<TimeCard> TimeCards { get; set; } = new();

		[ObservableProperty]
		public partial TimeCard? SelectedTimeCard { get; set; } = null;

		[ObservableProperty]
		public partial int SelectedNumberOfResults { get; set; } = 20;
		partial void OnSelectedNumberOfResultsChanged(int oldValue, int newValue)
		{
			if (oldValue != newValue)
				Task.Run(async () => await RefreshAllCards());
		}

        [ObservableProperty]
        public partial bool UseDateFilter { get; set; }
        [ObservableProperty]
        public partial DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        [ObservableProperty]
        public partial DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        [RelayCommand]
		public async Task OnAppearing()
		{
            PickerMinDate = cardService.GetAppFirstRunDate();
			await RefreshAllCards();
		}

		[RelayCommand]
		public async Task RefreshAllCards()
		{
			Loading = true;
			HasError = false;
			try
			{
				if (TimeCards.Count > 0)
					TimeCards.Clear();

				if (UseDateFilter)
				{
                    if (StartDate > EndDate)
                    {
                        await App.AlertSvc!.ShowAlertAsync("VALIDATION ERROR", "StartDate must be before EndDate").ConfigureAwait(false);
                        return;
                    }
					await Task.Run(async () =>
					{
						List<TimeCard> t = await cardService.GetTimeCards(SelectedNumberOfResults, StartDate, EndDate);
						TimeCards = t.ToObservableCollection();
					});
				}
				else
				{
					await Task.Run(async () =>
					{
						List<TimeCard> t = await cardService.GetTimeCards(SelectedNumberOfResults);
						TimeCards = t.ToObservableCollection();
					});
				}
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"\nEXCEPTION ERROR\n{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "TimeCardManagerViewModel");
			}
			finally
			{
				Loading = false;
			}
		}

        [RelayCommand]
        private void OnToggleDateFilterOptions()
        {
            UseDateFilter = !UseDateFilter;
        }
		
        [RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}
	}
}
