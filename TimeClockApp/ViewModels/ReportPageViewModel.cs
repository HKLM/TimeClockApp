using CommunityToolkit.Maui.Core.Extensions;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ReportPageViewModel(ReportPageService service) : TimeStampViewModel
    {
        protected readonly ReportPageService reportData = service;

        private List<TimeSheet> TimeSheetList = [];

        [ObservableProperty]
        private bool useDateFilter = false;
        [ObservableProperty]
        private DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        private DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        [ObservableProperty]
        private double regTotalHours = 0;
        [ObservableProperty]
        private double totalOTHours = 0;
        [ObservableProperty]
        private double totalOT2Hours = 0;
        [ObservableProperty]
        private double totalWorkHours = 0;
        [ObservableProperty]
        private double totalGrossPay = 0.00;
        partial void OnTotalGrossPayChanged(double value)
        {
            TotalEstimatedWC = value * WCRate;
        }

        [ObservableProperty]
        private double totalEstimatedWC = 0.00;
        partial void OnTotalEstimatedWCChanged(double value)
        {
            TotalLaborBurden = TotalEstimatedWC + TotalGrossPay;
        }
        public double WCRate { get; set; } = 0.00;

        [ObservableProperty]
        private double totalLaborBurden = 0.00;

        [ObservableProperty]
        private bool useEmployeeFilter = false;
        partial void OnUseEmployeeFilterChanged(bool value)
        {
            if (value == true)
                RefreshEmployees();
        }
        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = [];
        [ObservableProperty]
        private Employee? selectedEmployee = null;

        [ObservableProperty]
        private bool useProjectFilter = false;
        partial void OnUseProjectFilterChanged(bool value)
        {
            if (value == true)
                RefreshProjects();
        }
        [ObservableProperty]
        private ObservableCollection<Project> projectList = [];
        [ObservableProperty]
        private Project? selectedProject = null;

        [ObservableProperty]
        private bool usePhaseFilter = false;
        partial void OnUsePhaseFilterChanged(bool value)
        {
            if (value == true)
                RefreshPhases();
        }
        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = [];
        [ObservableProperty]
        private Phase? selectedPhase = null;

        public void OnAppearing()
        {
            PickerMinDate = reportData.GetAppFirstRunDate();
            WCRate = reportData.GetWCRate();
        }

        private void RefreshProjects()
        {
            ProjectList ??= [];
            ProjectList = reportData.GetProjectsList();
        }
        private void RefreshPhases()
        {
            PhaseList ??= [];
            PhaseList = reportData.GetPhaseList();
        }

        private void RefreshEmployees()
        {
            EmployeeList ??= [];
            if (EmployeeList.Count > 0)
                EmployeeList.Clear();

            EmployeeList = reportData.GetEmployeesList().ToObservableCollection();
        }

        [RelayCommand]
        private async Task MakeReport()
        {
            await Task.Run(async () => await RunReportAsync(UseEmployeeFilter, UseProjectFilter, UsePhaseFilter, UseDateFilter, SelectedEmployee, SelectedProject, SelectedPhase, StartDate, EndDate));
        }

        private async Task RunReportAsync(bool useEmployeeFilter, bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, Employee? employee, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            ResetItems();
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
