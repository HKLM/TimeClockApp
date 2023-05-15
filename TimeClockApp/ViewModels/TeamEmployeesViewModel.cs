namespace TimeClockApp.ViewModels
{
    public partial class TeamEmployeesViewModel : TimeStampViewModel
    {
        protected readonly UserManagerService employeeService;

        [ObservableProperty]
        private ObservableCollection<Employee> employee_List = new();

        public TeamEmployeesViewModel(UserManagerService service)
        {
            employeeService = service;
        }

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                employeeService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
