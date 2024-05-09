namespace TimeClockApp.ViewModels
{
    public partial class PayrollPageViewModel : TimeStampViewModel
    {
        protected readonly PayrollService payrollData;

        [ObservableProperty]
        private ObservableCollection<TimeSheet> sheetList = new ObservableCollection<TimeSheet>();
        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        private DateTime startDate;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        private DateTime endDate = DateTime.Now;
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        public string PayPeriod => $"Pay Period from {StartDate.ToShortDateString()} to {EndDate.ToShortDateString()}";

        /// <summary>
        /// Tracks if the filter options are displayed or not
        /// </summary>
        [ObservableProperty]
        private bool showFilterOptions = false;

        public PayrollPageViewModel(PayrollService service)
        {
            payrollData = service;
            ErrorMsg = string.Empty;
        }

        public void OnAppearing()
        {
            if (initDone)
            {
                try
                {
                    Refresh_Cards();
                }
                catch
                {
                    HasError = true;
                }
            }
        }

        [RelayCommand]
        private async Task InitAsync()
        {
            Loading = true;
            HasError = false;

            try
            {
                StartDate = payrollData.GetStartOfPayPeriod(DateTime.Now);
                PickerMinDate = payrollData.GetAppFirstRunDate();

                await RefreshEverythingAsync();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMsg = ex.Message;
            }
            finally
            {
                initDone = true;
                Loading = false;
            }
        }

        private async Task RefreshEmployeesAsync()
        {
            EmployeeList ??= [];
            if (EmployeeList.Count > 0)
                EmployeeList.Clear();

            Task<ObservableCollection<Employee>> g = payrollData.GetEmployeesAsync();
            EmployeeList = await g;
        }

        private void Refresh_Cards()
        {
            if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
            {
                App.AlertSvc.ShowAlert("VALIDATION ERROR", "StartDate must be before EndDate");
                return;
            }
            ResetSheets();
            foreach (Employee e in EmployeeList)
            {
                TimeSheet t = payrollData.GetPayrollTimeSheetForEmployee(e.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate), e.Employee_Name, null);
                if (t != null)
                    SheetList.Add(t);
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

        [RelayCommand]
        private async Task RefreshEverythingAsync()
        {
            Task A = RefreshEmployeesAsync();
            Task B = RefreshCardsAsync();
            await A;
            await B;
        }

        private void ResetSheets()
        {
            SheetList ??= new();
            SheetList.Clear();
        }

        [RelayCommand]
        private async Task MarkPaid(TimeSheet sheet)
        {
            if (sheet != null)
            {
                foreach (TimeCard item in sheet.TimeCards)
                {
                    if (item != null)
                    {
                        Task<bool> b = payrollData.MarkTimeCardAsPaidAsync(item);
                        if (await b)
                            System.Diagnostics.Debug.WriteLine("Marked card as paid");
                    }
                }

                Task refreshed = RefreshCardsAsync();
                await refreshed;
            }
        }

        [RelayCommand]
        private void ToggleFilterOptions()
        {
            ShowFilterOptions = !ShowFilterOptions;
            if (!ShowFilterOptions)
            {
                EndDate = DateTime.Now;
                StartDate = payrollData.GetStartOfPayPeriod(DateTime.Now);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox() => HelpInfoBoxVisible = !HelpInfoBoxVisible;
    }
}
