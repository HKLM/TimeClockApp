namespace TimeClockApp.ViewModels
{
    public partial class EditTimeCardHomeViewModel : TimeStampViewModel
    {
        protected EditTimeCardService cardService;
        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCardsHome = new();
        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = new();

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

        public EditTimeCardHomeViewModel()
        {
            cardService = new();
            EmployeeList = cardService.GetAllEmployees();
        }

        public void OnAppearing()
        {
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
                    if (TimeCardsHome.Any())
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
                if (TimeCardsHome.Any())
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                cardService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
