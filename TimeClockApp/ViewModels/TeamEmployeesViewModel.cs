#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TeamEmployeesViewModel : BaseViewModel
    {
        protected readonly UserManagerService employeeService = new();

        [ObservableProperty]
        public partial ObservableCollection<Employee> Employee_List { get; set; } = [];

        public async Task OnAppearing()
        {
            List<Employee> e = await employeeService.GetEmployeesGroupInStatusAsync();
            Employee_List = e.ToObservableCollection<Employee>();
        }

        [RelayCommand]
        private async Task SetTeamActiveAsync(Employee? employee)
        {
            if (employee == null) return;
            bool u = employeeService.UpdateEmployee(employee.EmployeeId, EmploymentStatus.Employed);
            if (u)
            {
                App.NoticeUserHasChanged = true;
                List<Employee> e = await employeeService.GetEmployeesGroupInStatusAsync();
                Employee_List = e.ToObservableCollection<Employee>();
            }
            else
                App.AlertSvc!.ShowAlert("ERROR", "Data not saved. Error in saving the data.");
        }

        [RelayCommand]
        private async Task SetTeamNotActiveAsync(Employee? employee)
        {
            if (employee == null) return;
            bool u = employeeService.UpdateEmployee(employee.EmployeeId, EmploymentStatus.Inactive);
            if (u)
            {
                App.NoticeUserHasChanged = true;
                List<Employee> e = await employeeService.GetEmployeesGroupInStatusAsync();
                Employee_List = e.ToObservableCollection<Employee>();
            }
            else
                App.AlertSvc!.ShowAlert("ERROR", "Something went wrong. Data not saved.");
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
