namespace TimeClockApp.ViewModels
{
    public partial class TeamEmployeesViewModel(UserManagerService service) : TimeStampViewModel
    {
        protected readonly UserManagerService employeeService = service;

        [ObservableProperty]
        private ObservableCollection<Employee> employee_List = [];

        public void OnAppearing()
        {
            RefreshEmployeeList();
        }

        private void RefreshEmployeeList()
        {
            if (Employee_List.Any())
                Employee_List.Clear();
            Employee_List = employeeService.GetEmployeesGroupInStatus();
        }

#nullable enable

        [RelayCommand]
        private void SetTeamActive(Employee? employee)
        {
            if (employee?.EmployeeId > 0)
            {
                if (employeeService.UpdateEmployee(employee.EmployeeId, EmploymentStatus.Employed))
                {
                    App.NoticeUserHasChanged = true;
                    RefreshEmployeeList();
                    App.AlertSvc.ShowAlert("NOTICE", "Saved " + employee.Employee_Name);
                }
                else
                    App.AlertSvc.ShowAlert("ERROR", "Something went wrong. Data not saved.");
            }
        }

        [RelayCommand]
        private void SetTeamNotActive(Employee? employee)
        {
            if (employee?.EmployeeId > 0)
            {
                if (employeeService.UpdateEmployee(employee.EmployeeId, EmploymentStatus.Inactive))
                {
                    App.NoticeUserHasChanged = true;
                    RefreshEmployeeList();
                    App.AlertSvc.ShowAlert("NOTICE", "Saved " + employee.Employee_Name);
                }
                else
                    App.AlertSvc.ShowAlert("ERROR", "Something went wrong. Data not saved.");
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
