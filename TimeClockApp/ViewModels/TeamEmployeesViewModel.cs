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
			Employee_List = e.ToObservableCollection();
		}

		[RelayCommand]
		private async Task SetTeamActiveAsync(Employee? employee)
		{
			if (employee == null) return;
			if (employeeService.UpdateEmployee(employee.EmployeeId, EmploymentStatus.Employed))
			{
				List<Employee> e = await employeeService.GetEmployeesGroupInStatusAsync();
				Employee_List = e.ToObservableCollection();
			}
			else
				await App.AlertSvc!.ShowAlertAsync("ERROR", "Data not saved. Error in saving the data.").ConfigureAwait(false);
		}

		[RelayCommand]
		private async Task SetTeamNotActiveAsync(Employee? employee)
		{
			if (employee == null) return;
			if (employeeService.UpdateEmployee(employee.EmployeeId, EmploymentStatus.Inactive))
			{
				List<Employee> e = await employeeService.GetEmployeesGroupInStatusAsync();
				Employee_List = e.ToObservableCollection();
			}
			else
				await App.AlertSvc!.ShowAlertAsync("ERROR", "Something went wrong. Data not saved.").ConfigureAwait(false);
		}

		[RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}
	}
}
