using TimeClockApp.Shared.Interfaces;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class InvoiceDetailExpensesViewModel(TimeCardService timeCardService, ISharedService sharedServ) : BaseViewModel
    {
        protected readonly TimeCardService cardService = timeCardService;
        protected readonly ISharedService _SharedService = sharedServ;

        [ObservableProperty]
        public partial ObservableCollection<Expense> ExpenseList { get; set; } = [];

        [RelayCommand]
        public void OnAppearing()
        {
            try
            {
                List<Expense> e = _SharedService.GetValue<List<Expense>>("ExpenseList");
                ExpenseList = e.ToObservableCollection();
                _SharedService.Add<List<Expense>>("ExpenseList", new());
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
