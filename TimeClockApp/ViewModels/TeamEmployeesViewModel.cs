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
            try
            {
                if (employee?.EmployeeId > 0)
                {
                    if (employeeService.UpdateEmploymentStatus(employee, EmploymentStatus.Employed))
                    {
                        App.NoticeUserHasChanged = true;
                        RefreshEmployeeList();
                        App.AlertSvc.ShowAlert("NOTICE", "Saved " + employee.Employee_Name);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void SetTeamNotActive(Employee? employee)
        {
            try
            {
                if (employee?.EmployeeId > 0)
                {
                    if (employeeService.UpdateEmploymentStatus(employee, EmploymentStatus.Inactive))
                    {
                        App.NoticeUserHasChanged = true;
                        RefreshEmployeeList();
                        App.AlertSvc.ShowAlert("NOTICE", "Saved " + employee.Employee_Name);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
