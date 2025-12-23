namespace TimeClockApp.ViewModels
{
    public partial class PayrollPageViewModel : BaseViewModel
    {
        protected readonly PayrollService payrollData;

        [ObservableProperty]
        public partial ObservableCollection<TimeSheet> SheetList { get; set; } = [];
        [ObservableProperty]
        public partial ObservableCollection<Employee> EmployeeList { get; set; } = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        public partial DateTime EndDate { get; set; } = DateTime.Now;
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
        public partial bool ShowFilterOptions { get; set; } = false;

        public PayrollPageViewModel(PayrollService service)
        {
            payrollData = service;
            ErrorMsg = string.Empty;
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            Loading = true;
            HasError = false;

            try
            {
                StartDate = payrollData.GetStartOfPayPeriod(DateTime.Now);
                PickerMinDate = payrollData.GetAppFirstRunDate();

                List<Employee> g = await payrollData.GetEmployeeListAsync();
                EmployeeList = g.ToObservableCollection();
                if (g.Count > 0)
                {
                    foreach (Employee e in EmployeeList)
                    {
                        TimeSheet t = payrollData.GetPayrollTimeSheetForEmployee(e.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate), e.Employee_Name, null);
                        if (t != null)
                            SheetList.Add(t);
                    }
                }
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMsg = ex.Message;
            }
            finally
            {
                Loading = false;
            }
        }

        private async Task RefreshEmployeesAsync()
        {
            if (App.NoticeUserHasChanged || EmployeeList.Count > 0)
            {
                EmployeeList.Clear();
                List<Employee> g = await payrollData.GetEmployeeListAsync();
                EmployeeList =  g.ToObservableCollection();
            }
        }

        [RelayCommand]
        private async Task RefreshCardsAsync()
        {
            if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
            {
                await App.AlertSvc!.ShowAlertAsync("VALIDATION ERROR", "StartDate must be before EndDate");
                return;
            }
            SheetList.Clear();
            foreach (Employee e in EmployeeList)
            {
                TimeSheet t = payrollData.GetPayrollTimeSheetForEmployee(e.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate), e.Employee_Name, null);
                if (t != null)
                    SheetList.Add(t);
            }
        }

        [RelayCommand]
        private async Task RefreshEverythingAsync()
        {
            Task A = RefreshEmployeesAsync();
            Task B = RefreshCardsAsync();
            await A;
            await B;
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
                            Log.WriteLine("Marked card as paid");
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
