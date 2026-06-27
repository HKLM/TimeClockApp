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
				Task.Run(async () => await RefreshAllCards().ContinueWith(task =>
				{
					if (task.Exception != null)
					{
						Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "TimeCardManagerViewModel.OnSelectedNumberOfResultsChanged");
					}
				}));
		}


		[RelayCommand]
		public async Task OnAppearing()
		{
			if (Loading)
				return;

			Loading = true;
			HasError = false;

			try
			{
				await Task.Run(async () =>
				{
					List<TimeCard> t = await cardService.GetTimeCards(SelectedNumberOfResults);
					TimeCards = t.ToObservableCollection();
				});
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
		private async Task RefreshAllCards()
		{
			if (Loading)
				return;

			Loading = true;
			HasError = false;
			try
			{
				if (TimeCards.Count > 0)
					TimeCards.Clear();
				await Task.Run(async () =>
				{
					List<TimeCard> t = await cardService.GetTimeCards(SelectedNumberOfResults);
					TimeCards = t.ToObservableCollection();
				});
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
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}
	}
}
