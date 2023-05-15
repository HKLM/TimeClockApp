namespace TimeClockApp.ViewModels
{
    public partial class ReportWeekViewModel : TimeStampViewModel, IDisposable
    {
        protected readonly ReportDataService reportData;
        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCards = new();
        [ObservableProperty]
        private ObservableCollection<Wages> wagesList = new();
        [ObservableProperty]
        private ObservableCollection<Wages> owedWagesList = new();

        [ObservableProperty]
        private Employee selectedFilter;
        partial void OnSelectedFilterChanged(global::TimeClockApp.Models.Employee value)
        {
            Refresh_Cards();
        }

        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = new();

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject;

        [ObservableProperty]
        private double regTotalHours = 0;
        [ObservableProperty]
        private double totalOTHours = 0;
        [ObservableProperty]
        private double totalOT2Hours = 0;
        [ObservableProperty]
        private double regTotalPay = 0;
        [ObservableProperty]
        private double totalOTPay = 0;
        [ObservableProperty]
        private double totalOT2Pay = 0;

        [ObservableProperty]
        private double totalWorkHours = 0;
        [ObservableProperty]
        private double totalPaidWorkHours = 0;
        [ObservableProperty]
        private double totalGrossPay = 0.00;

        [ObservableProperty]
        private double totalOwedGrossPay = 0.00;
        [ObservableProperty]
        private DateTime startDate = DateTime.Now;
        [ObservableProperty]
        private DateTime endDate = DateTime.Now;

        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion

        [ObservableProperty]
        private bool displayLandscapeMode = false;
        partial void OnDisplayLandscapeModeChanged(bool value)
        {
            NotDisplayLandscapeMode = !DisplayLandscapeMode;
        }

        [ObservableProperty]
        private bool notDisplayLandscapeMode = true;
        /// <summary>
        /// Tracks if the filter options are displayed or not
        /// </summary>
        [ObservableProperty]
        private bool showFilterOptions = false;

        public ReportWeekViewModel(ReportDataService service)
        {
            reportData = service;
            ShowFilterOptions = false;
            EndDate = DateTime.Now;
            StartDate = reportData.GetStartOfPayPeriod(DateTime.Now);
            PickerMinDate = reportData.GetAppFirstRunDate();
        }

        public async Task OnAppearingAsync()
        {
            LoadProjects();
            Task A = RefreshEmployeesAsync();
            Task B = RefreshCardsAsync();
            await A;
            await B;
        }

        private async Task RefreshEmployeesAsync()
        {
            EmployeeList ??= new();
            if (EmployeeList.Any() == false || App.NoticeUserHasChanged == true)
            {
                Task<ObservableCollection<Employee>> g = reportData.GetEmployeesAsync();
                EmployeeList = await g;
            }
        }

        private void Refresh_Cards()
        {
            ResetWageItems();
            if (SelectedFilter != null && SelectedFilter.EmployeeId > 0)
            {
                if (ShowFilterOptions && SelectedProject != null)
                    (TimeCards, WagesList, OwedWagesList) = reportData.GetReportTimeCardsForEmployee(SelectedFilter.EmployeeId, SelectedProject.ProjectId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));
                else
                    (TimeCards, WagesList, OwedWagesList) = reportData.GetReportTimeCardsForEmployee(SelectedFilter.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));

                (RegTotalHours, TotalOTHours, TotalOT2Hours, TotalWorkHours) = reportData.GetHours(WagesList.ToList());
                (RegTotalPay, TotalOTPay, TotalOT2Pay, TotalGrossPay) = reportData.GetPay(WagesList.ToList());
                (_, _, _, TotalOwedGrossPay) = reportData.GetPay(OwedWagesList.ToList());
            }
        }

        [RelayCommand]
        private Task RefreshCardsAsync()
        {
            return Task.Run(() =>
            {
                Refresh_Cards();
                return Task.CompletedTask;
            });
        }

        private void ResetWageItems()
        {
            TimeCards.Clear();
            WagesList.Clear();
            OwedWagesList.Clear();

            RegTotalHours = 0;
            TotalOTHours = 0;
            TotalOT2Hours = 0;
            TotalWorkHours = 0;
            RegTotalPay = 0;
            TotalOTPay = 0;
            TotalOT2Pay = 0;
            TotalGrossPay = 0;
            TotalOwedGrossPay = 0;
        }

        private void LoadProjects()
        {
            ProjectList ??= new();
            if (ProjectList.Any() == false || App.NoticeProjectHasChanged == true)
                ProjectList = reportData.GetAllProjectsList(true);
        }

        [RelayCommand]
        private async Task LoadTimeCardsAsync()
        {
            if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
            {
                await App.AlertSvc.ShowAlertAsync("VALIDATION ERROR", "StartDate must be before EndDate");
                return;
            }
            if (SelectedFilter == null || SelectedFilter.EmployeeId == 0)
            {
                await App.AlertSvc.ShowAlertAsync("VALIDATION ERROR", "You must select a employee first");
                return;
            }

            Task loadCards = RefreshCardsAsync();
            await loadCards;
        }

        [RelayCommand]
        private async Task MarkPaidAsync(TimeCard card)
        {
            if (card == null)
                return;

            Task<bool> m = reportData.MarkTimeCardAsPaidAsync(card);
            if (await m)
            {
                Task refreshed = RefreshCardsAsync();
                await refreshed;
            }
        }

        [RelayCommand]
        private async Task MarkAllPaidAsync()
        {
            foreach (TimeCard item in TimeCards)
            {
                Task<bool> b = reportData.MarkTimeCardAsPaidAsync(item);
                if (await b)
                    System.Diagnostics.Debug.WriteLine("Marked card as paid");
            }
            Task refreshed = RefreshCardsAsync();
            await refreshed;
        }


        [RelayCommand]
        private async Task GetAllUnpaidTimeCards()
        {
            if (SelectedFilter != null && SelectedFilter.EmployeeId > 0)
            {
                ResetWageItems();
                (TimeCards, WagesList, OwedWagesList) = await reportData.GetAllUnpaidTimeCardsForEmployeeAsync(SelectedFilter.EmployeeId);
                (RegTotalHours, TotalOTHours, TotalOT2Hours, TotalWorkHours) = reportData.GetHours(WagesList.ToList());
                (RegTotalPay, TotalOTPay, TotalOT2Pay, TotalGrossPay) = reportData.GetPay(WagesList.ToList());
                (_, _, _, TotalOwedGrossPay) = reportData.GetPay(OwedWagesList.ToList());
            }
        }

        [RelayCommand]
        private async Task ReportClockOut(TimeCard card)
        {
            if (card == null)
                return;

            Task<bool> b = reportData.EmployeeClockOutAsync(card);
            if (await b)
            {
                Task refreshed = RefreshCardsAsync();
                await refreshed;
            }
        }

        [RelayCommand]
        private void ChangeDisplayLandscapeMode(bool isLandscapeMode)
        {
            DisplayLandscapeMode = isLandscapeMode;
        }

        [RelayCommand]
        private void ToggleFilterOptions()
        {
            ShowFilterOptions = !ShowFilterOptions;
            if (!ShowFilterOptions)
            {
                SelectedProject = null;
                EndDate = DateTime.Now;
                StartDate = reportData.GetStartOfPayPeriod(DateTime.Now);
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
                reportData.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
