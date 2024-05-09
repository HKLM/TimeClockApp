namespace TimeClockApp.ViewModels
{
    public partial class EditTimeCardHomeViewModel(EditTimeCardService _cardService) : TimeStampViewModel
    {
        protected EditTimeCardService cardService = _cardService;

        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCardsHome = [];
        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = [];

        [ObservableProperty]
        private Employee selectedFilter;
        partial void OnSelectedFilterChanged(Employee value)
        {
            GetTimeCardList();
        }

        [ObservableProperty]
        private bool refreshingData;

        [ObservableProperty]
        private bool showPaid = false;
        partial void OnShowPaidChanged(bool value)
        {
            GetTimeCardList();
        }

        public void OnAppearing()
        {
            EmployeeList = cardService.GetAllEmployees();

            GetTimeCardList();
        }

        [RelayCommand]
        private void LoadTimeCards()
        {
            RefreshingData = true;
            try
            {
                if (SelectedFilter != null && SelectedFilter.EmployeeId != 0)
                {
                    if (TimeCardsHome.Count > 0)
                        TimeCardsHome.Clear();
                    TimeCardsHome = cardService.GetTimeCardsForEmployee(SelectedFilter.EmployeeId, ShowPaid);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                RefreshingData = false;
            }
            RefreshingData = false;
        }

        private void GetTimeCardList()
        {
            if (SelectedFilter != null && SelectedFilter.EmployeeId != 0)
            {
                if (TimeCardsHome.Count > 0)
                    TimeCardsHome.Clear();

                try
                {
                    TimeCardsHome = cardService.GetTimeCardsForEmployee(SelectedFilter.EmployeeId, ShowPaid);
                }
                catch (Exception ex)
                {
                    App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
                }
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
