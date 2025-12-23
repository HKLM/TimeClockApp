#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TimeCardManagerViewModel(EditTimeCardService cardService) : BaseViewModel
    {
        private readonly EditTimeCardService cardService = cardService;

        [ObservableProperty]
        public partial ObservableCollection<TimeCard> TimeCards { get; set; } = new();

        [ObservableProperty]
        public partial int SelectedNumberOfResults { get; set; } = 20;

        [RelayCommand]
        public async Task OnAppearing()
        {
            if (Loading) 
                return;

            Loading = true;
            HasError = false;

            try
            {
                await GetCardsAsync();
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine(e.Message + "\n  -- " + e.Source + "\n  -- " + e.InnerException);
            }
            finally
            {
                Loading = false;
            }
        }

        private Task GetCardsAsync() => Task.Run(async () =>
                                                       {
                                                           List<TimeCard> t = await cardService.GetTimeCards(SelectedNumberOfResults);
                                                           TimeCards = t.ToObservableCollection();
                                                       });

        [RelayCommand]
        private async Task RefreshAllCards()
        {
            if (Loading) 
                return;

            Loading = true;
            HasError = false;

            if (TimeCards.Count > 0)
                TimeCards.Clear();
            try
            {
                await GetCardsAsync();
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine(e.Message + "\n  -- " + e.Source + "\n  -- " + e.InnerException);
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
