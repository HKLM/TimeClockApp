#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ReportPageViewModel : BaseViewModel
    {
        protected readonly ReportPageService reportData = new();

        private List<TimeSheet> TimeSheetList = [];

        [ObservableProperty]
        public partial bool UseDateFilter { get; set; } = false;
        [ObservableProperty]
        public partial DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        public partial DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion

        [ObservableProperty]
        public partial double RegTotalHours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalOTHours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalOT2Hours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalWorkHours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalGrossPay { get; set; } = 0.00;
        partial void OnTotalGrossPayChanged(double value)
        {
            TotalEstimatedWC = value * WCRate;
        }

        [ObservableProperty]
        public partial double TotalEstimatedWC { get; set; } = 0.00;
        partial void OnTotalEstimatedWCChanged(double value)
        {
            TotalLaborBurden = TotalEstimatedWC + TotalGrossPay;
        }
        public double WCRate { get; set; } = 0.00;

        [ObservableProperty]
        public partial double TotalLaborBurden { get; set; } = 0.00;

        [ObservableProperty]
        public partial bool UseEmployeeFilter { get; set; } = false;
        partial void OnUseEmployeeFilterChanged(bool value)
        {
            if (value == true)
                RefreshEmployees();
        }
        [ObservableProperty]
        public partial ObservableCollection<Employee> EmployeeList { get; set; } = new();
        /// <summary>
        /// List of Employees to filter by
        /// </summary>
        private List<Employee> SelectedEmployeeList = new();

        [ObservableProperty]
        public partial Employee? SelectedEmployee { get; set; } = null;

        [ObservableProperty]
        public partial bool UseProjectFilter { get; set; } = false;
        partial void OnUseProjectFilterChanged(bool value)
        {
            if (value == true)
                RefreshProjects();
        }
        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = [];
        [ObservableProperty]
        public partial Project? SelectedProject { get; set; } = null;

        [ObservableProperty]
        public partial bool UsePhaseFilter { get; set; } = false;
        partial void OnUsePhaseFilterChanged(bool value)
        {
            if (value == true)
                RefreshPhases();
        }
        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = [];

        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;

        public void OnAppearing()
        {
            PickerMinDate = reportData.GetAppFirstRunDate();
            StartDate = DateOnly.FromDateTime(reportData.GetStartOfPayPeriod(DateTime.Now));
            WCRate = reportData.GetWCRate();
            List<Employee> e = reportData.GetEmployeesList();
            EmployeeList = new ObservableCollection<Employee>(e as List<Employee>);
        }

        private void RefreshProjects()
        {
            ProjectList = reportData.GetProjectsList();
        }
        private void RefreshPhases()
        {
            PhaseList = reportData.GetPhaseList();
        }

        private void RefreshEmployees()
        {
            List<Employee> e = reportData.GetEmployeesList();
            EmployeeList = new ObservableCollection<Employee>(e as List<Employee>);
        }

        [RelayCommand]
        private Task MakeReport()
        {
            ResetItems();
            return Task.Run(async () => await RunReportAsync(UseEmployeeFilter, UseProjectFilter, UsePhaseFilter, UseDateFilter, SelectedEmployeeList, SelectedProject, SelectedPhase, StartDate, EndDate));
        }

        private async Task RunReportAsync(bool useEmployeeFilter, bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, List<Employee> employee, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {

            Task<List<TimeSheet>> t = reportData.RunFullReportAsync(useEmployeeFilter, useProjectFilter, usePhaseFilter, useDateFilter, employee, project, phase, start, end);
            TimeSheetList = await t;
            foreach (TimeSheet i in TimeSheetList)
            {
                TotalGrossPay += i.TotalGrossPay;
                TotalWorkHours += i.TotalWorkHours;
                TotalOTHours += i.TotalOTHours;
                TotalOT2Hours += i.TotalOT2Hours;
            }
        }

        [RelayCommand]
        private void ClickedEmployeeList(int employeeId)
        {
            int idx = SelectedEmployeeList.FindIndex(x => x.EmployeeId == employeeId);
            if (idx > 0)
            {
                SelectedEmployeeList.RemoveAt(idx);
            }
            else
            {
                Employee? e = reportData.GetEmployee(employeeId);
                SelectedEmployeeList.Add(e);
            }
            ResetItems();
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        private void ResetItems()
        {
            RegTotalHours = 0;
            TotalOTHours = 0;
            TotalOT2Hours = 0;
            TotalWorkHours = 0;
            TotalGrossPay = 0;
        }
    }
}
