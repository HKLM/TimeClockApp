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
        public partial double PayRate { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Employee> EmployeeList { get; set; } = [];

        [ObservableProperty]
        public partial Employee SelectedEmployee { get; set; }
        partial void OnSelectedEmployeeChanged(global::TimeClockApp.Shared.Models.Employee value)
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
            try
            {
                if (!string.IsNullOrEmpty(EmployeeName))
                {
                    string employeeNewName = EmployeeName.Trim();
                    string employeeNewJobTitle = JobTitle.Trim();
                    HRService.AddNewEmployee(employeeNewName, PayRate, employeeNewJobTitle, IsEmployed);
                    await RefreshEmployeesAsync();
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Added new employee " + employeeNewName).ConfigureAwait(false);
                }
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task FireEmployeeAsync()
        {
            try
            {
                if (SelectedEmployee != null)
                {
                    string eName = SelectedEmployee.Employee_Name;
                    HRService.FireEmployee(SelectedEmployee.EmployeeId);
                    await RefreshEmployeesAsync();
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", eName + " is Fired!").ConfigureAwait(false);
                    RefreshInfo();
                }
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task SaveEditEmployeeAsync()
        {
            if (string.IsNullOrEmpty(EmployeeName))
                return;

            try
            {
                string employeeNewName = EmployeeName.Trim();
                string employeeNewJobTitle = JobTitle.Trim();
                if (EmployeeId > 0 && HRService.UpdateEmployee(EmployeeId, employeeNewName, PayRate, IsEmployed, employeeNewJobTitle))
                {
                    await RefreshEmployeesAsync();
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Saved " + employeeNewName).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
