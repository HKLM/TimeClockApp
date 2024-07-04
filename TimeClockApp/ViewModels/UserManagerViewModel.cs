﻿namespace TimeClockApp.ViewModels
{
    public partial class UserManagerViewModel : TimeStampViewModel
    {
        protected readonly UserManagerService HRService;
        [ObservableProperty]
        private int employeeId = 0;
        [ObservableProperty]
        private string employeeName;
        partial void OnEmployeeNameChanging(string value)
        {
            EnableAddButton = !string.IsNullOrEmpty(EmployeeName);
        }

        [ObservableProperty]
        private string jobTitle;
        [ObservableProperty]
        private double payRate;

        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = [];

        [ObservableProperty]
        private Employee selectedEmployee;
        partial void OnSelectedEmployeeChanged(global::TimeClockApp.Shared.Models.Employee value)
        {
            RefreshInfo();
            EnableSaveDelButton = SelectedEmployee != null && SelectedEmployee.EmployeeId > 0;
        }

        [ObservableProperty]
        private EmploymentStatus isEmployed;
        public IReadOnlyList<string> AllCategory { get; } = Enum.GetNames(typeof(EmploymentStatus));
        [ObservableProperty]
        public bool enableSaveDelButton;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Enable_AddButton))]
        public bool enableAddButton;
        public bool Enable_AddButton
        {
            get => EnableAddButton;
            set => EnableAddButton = (EmployeeId > 0);
        }

        public UserManagerViewModel()
        {
            HRService = new();
        }

        public void OnAppearing()
        {
            RefreshEmployees(false);
        }

        private void RefreshEmployees(bool isUpdating)
        {
            if (isUpdating)
            {
                App.NoticeUserHasChanged = true;
            }
            EmployeeList = HRService.GetAllEmployees(true);
        }

        private async Task RefreshEmployeesAsync(bool isUpdating)
        {
            if (isUpdating)
            {
                App.NoticeUserHasChanged = true;
            }
            EmployeeList = await HRService.GetAllEmployeesAsync(true);
        }

        [RelayCommand]
        private async Task LoadEmployeesAsync()
        {
            try
            {
                await RefreshEmployeesAsync(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
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
                if (EmployeeName != null && EmployeeName != "" && JobTitle != null && JobTitle != "")
                {
                    HRService.AddNewEmployee(EmployeeName, PayRate, JobTitle);
                    await RefreshEmployeesAsync(true);
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Added new employee " + EmployeeName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
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
                    await RefreshEmployeesAsync(true);
                    await App.AlertSvc.ShowAlertAsync("NOTICE", eName + " is Fired!");
                    RefreshInfo();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }


        [RelayCommand]
        private void SaveEditEmployee()
        {
            try
            {
                if (EmployeeId > 0 && HRService.UpdateEmployee(EmployeeId, EmployeeName, PayRate, IsEmployed))
                {
                    RefreshEmployees(true);
                    App.AlertSvc.ShowAlert("NOTICE", "Saved " + EmployeeName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
