using CommunityToolkit.Maui.Core.Extensions;

namespace TimeClockApp.ViewModels
{
    public partial class TimeCardManagerViewModel(EditTimeCardService cardService) : TimeStampViewModel
    {
        protected readonly EditTimeCardService cardService = cardService;

        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCards = new();

        [ObservableProperty]
        private int selectedNumberOfResults = 10;

        [RelayCommand]
        private async Task InitAsync()
        {
            Loading = true;
            HasError = false;

            SelectedNumberOfResults = 20;

            TimeCards ??= new();
            try
            {
                await GetCardsAsync();
            }
            catch (Exception e)
            {
                HasError = true;
                System.Diagnostics.Trace.WriteLine(e.Message + "\n  -- " + e.Source + "\n  -- " + e.InnerException);
            }
            finally
            {
                Loading = false;
            }
        }

        private async Task GetCardsAsync() => await Task.Run(async () =>
                                                       {
                                                           List<TimeCard> t = await cardService.GetTimeCards(SelectedNumberOfResults);
                                                           if (t?.Count > 0)
                                                               TimeCards = t.ToObservableCollection();
                                                       });

        [RelayCommand]
        private async Task RefreshAllCards()
        {
            Loading = true;
            HasError = false;

            if (TimeCards.Count > 0)
                TimeCards.Clear();
            try
            {
                await GetCardsAsync();
            }
            catch (Exception e)
            {
                HasError = true;
                System.Diagnostics.Trace.WriteLine(e.Message + "\n  -- " + e.Source + "\n  -- " + e.InnerException);
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
