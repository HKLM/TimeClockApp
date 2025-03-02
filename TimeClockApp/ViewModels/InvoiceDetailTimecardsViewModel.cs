using TimeClockApp.Shared.Interfaces;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class InvoiceDetailTimecardsViewModel(TimeCardService timeCardService, ISharedService sharedServ) : BaseViewModel
    {
        protected readonly TimeCardService cardService = timeCardService;
        protected readonly ISharedService _SharedService = sharedServ;

        [ObservableProperty]
        public partial ObservableCollection<TimeCard> CardList { get; set; } = new();

        [RelayCommand]
        public void OnAppearing()
        {
            try
            {
                List<TimeCard> e = _SharedService.GetValue<List<TimeCard>>("CardList");
                CardList = e.ToObservableCollection();
                _SharedService.Add<List<TimeCard>>("CardList", new());
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
