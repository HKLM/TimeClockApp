#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class UserManagerViewModel : BaseViewModel
	{
		protected readonly UserManagerService HRService;
		[ObservableProperty]
		public partial int EmployeeId { get; set; } = 0;
		[ObservableProperty]
		public partial string EmployeeName { get; set; } = string.Empty;
		partial void OnEmployeeNameChanging(string value)
		{
			EnableAddButton = !string.IsNullOrEmpty(EmployeeName);
		}

		[ObservableProperty]
		public partial string JobTitle { get; set; } = string.Empty;
		[ObservableProperty]
		public partial double PayRate { get; set; } = 0.0;

		[ObservableProperty]
		public partial ObservableCollection<Employee> EmployeeList { get; set; } = [];

		[ObservableProperty]
		public partial Employee? SelectedEmployee { get; set; }
		partial void OnSelectedEmployeeChanged(global::TimeClockApp.Shared.Models.Employee? value)
		{
			RefreshInfo();
			EnableSaveDelButton = SelectedEmployee != null && SelectedEmployee.EmployeeId > 0;
		}

		[ObservableProperty]
		public partial EmploymentStatus IsEmployed { get; set; }
		public IReadOnlyList<string> AllCategory { get; } = Enum.GetNames(typeof(EmploymentStatus));
		[ObservableProperty]
		public partial bool EnableSaveDelButton { get; set; }
		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(Enable_AddButton))]
		public partial bool EnableAddButton { get; set; }
		public bool Enable_AddButton
		{
			get => EnableAddButton;
			set => EnableAddButton = (EmployeeId > 0);
		}

		public UserManagerViewModel()
		{
			HRService = new();
		}

		public async Task OnAppearing()
		{
			HasError = false;
			try
			{
				List<Employee> e = await HRService.GetAllEmployeesAsync();
				EmployeeList = e.ToObservableCollection();
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "UserManagerViewModel.OnAppearing");
			}
		}

		private async Task RefreshEmployeesAsync()
		{
			List<Employee> e = await HRService.GetAllEmployeesAsync();
			EmployeeList = e.ToObservableCollection();
		}

		[RelayCommand]
		private async Task LoadEmployeesAsync()
		{
			HasError = false;
			try
			{
				List<Employee> e = await HRService.GetAllEmployeesAsync();
				EmployeeList = e.ToObservableCollection();
			}
			catch (AggregateException ax)
			{
				HasError = true;
				TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
			}
			catch (Exception ex)
			{
				HasError = true;
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
			}
		}

		private void RefreshInfo()
		{
			if (SelectedEmployee != null)
			{
				Employee e = HRService.GetEmployee(SelectedEmployee.EmployeeId);
				if (e != null)
				{
					EmployeeName = e.Employee_Name;
					JobTitle = e.JobTitle;
					PayRate = e.Employee_PayRate;
					EmployeeId = e.EmployeeId;
					IsEmployed = e.Employee_Employed;
				}
			}
		}

		[RelayCommand]
		private async Task SaveNewEmployeeAsync()
		{
			HasError = false;
			try
			{
				if (!string.IsNullOrEmpty(EmployeeName))
				{
					string employeeNewName = EmployeeName.Trim();
					string employeeNewJobTitle = JobTitle.Trim();
					int i = HRService.AddNewEmployee(employeeNewName, PayRate, employeeNewJobTitle, IsEmployed);
					await RefreshEmployeesAsync();
					if (i > 0)
					{
						SelectedEmployee = EmployeeList.FirstOrDefault(x => x.EmployeeId == i);
					}

					await AppShell.DisplayToastAsync($"Added new employee {employeeNewName}");
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
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
			}
		}

		[RelayCommand]
		private async Task FireEmployeeAsync()
		{
			HasError = false;
			try
			{
				if (SelectedEmployee != null)
				{
					string eName = SelectedEmployee.Employee_Name;
					if (await HRService.FireEmployeeAsync(SelectedEmployee.EmployeeId))
					{
						await RefreshEmployeesAsync();
						RefreshInfo();

						await AppShell.DisplayToastAsync($"{eName} is Fired!");
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
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
			}
		}

		[RelayCommand]
		private async Task SaveEditEmployeeAsync()
		{
			HasError = false;
			if (string.IsNullOrEmpty(EmployeeName))
				return;

			try
			{
				string employeeNewName = EmployeeName.Trim();
				string employeeNewJobTitle = JobTitle.Trim();
				if (EmployeeId > 0 && HRService.UpdateEmployee(EmployeeId, employeeNewName, PayRate, IsEmployed, employeeNewJobTitle))
				{
					await RefreshEmployeesAsync();
					await AppShell.DisplayToastAsync($"Saved {employeeNewName}");
				}
			}
			catch (Exception ex)
			{
				HasError = true;
				Log.WriteLine(ex.Message + "\n" + ex.InnerException);
			}
		}

		[RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}

		[RelayCommand]
		private void ClearTheForm()
		{
			EmployeeName = string.Empty;
			JobTitle = string.Empty;
			PayRate = 0.0;
			IsEmployed = EmploymentStatus.Employed;
			SelectedEmployee = null;
			EmployeeId = 0;
		}
	}
}
