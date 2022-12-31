using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class UserManagerViewModel : TimeStampViewModel
    {
        protected UserManagerService HRservice;
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
        private ObservableCollection<Employee> employeeList = new();

        [ObservableProperty]
        private Employee selectedEmployee;
        partial void OnSelectedEmployeeChanged(global::TimeClockApp.Models.Employee value)
        {
            RefreshInfo();
            EnableSaveDelButton = SelectedEmployee != null && SelectedEmployee.EmployeeId > 0;
        }

        [ObservableProperty]
        private EmploymentStatus isEmployeed;
        public IReadOnlyList<string> AllCatagory { get; } = Enum.GetNames(typeof(EmploymentStatus));
        [ObservableProperty]
        public bool enableSaveDelButton;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(enable_AddButton))]
        public bool enableAddButton;
        public bool enable_AddButton
        {
            get => EnableAddButton;
            set => EnableAddButton = (EmployeeId > 0);
        }

        public UserManagerViewModel()
        {
            HRservice = new();
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
            EmployeeList = HRservice.GetAllEmployees(true);
        }

        private async Task RefreshEmployeesAsync(bool isUpdating)
        {
            if (isUpdating)
            {
                App.NoticeUserHasChanged = true;
            }
            EmployeeList = await HRservice.GetAllEmployeesAsync(true);
        }

        [RelayCommand]
        private async Task LoadEmployeesAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await RefreshEmployeesAsync(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void RefreshInfo()
        {
            if (SelectedEmployee != null)
            {
                Employee e = HRservice.GetEmployee(SelectedEmployee.EmployeeId);
                if (e != null)
                {
                    EmployeeName = e.Employee_Name;
                    JobTitle = e.JobTitle;
                    PayRate = e.Employee_PayRate;
                    EmployeeId = e.EmployeeId;
                    IsEmployeed = e.Employee_Employed;
                }
            }
        }

        [RelayCommand]
        private async Task SaveNewEmployeeAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (EmployeeName != null && EmployeeName != "" && JobTitle != null && JobTitle != "")
                {
                    HRservice.AddNewEmployee(EmployeeName, PayRate, JobTitle);
                    await RefreshEmployeesAsync(true);
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Added new employee " + EmployeeName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task FireEmployeeAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (SelectedEmployee != null)
                {
                    string eName = SelectedEmployee.Employee_Name;
                    HRservice.FireEmployee(SelectedEmployee.EmployeeId);
                    await RefreshEmployeesAsync(true);
                    await App.AlertSvc.ShowAlertAsync("NOTICE", eName + " is Fired!");
                    RefreshInfo();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }


        [RelayCommand]
        private void SaveEditEmployee()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (EmployeeId > 0 && HRservice.UpdateEmployee(EmployeeId, EmployeeName, PayRate, IsEmployeed))
                {
                    RefreshEmployees(true);
                    App.AlertSvc.ShowAlert("NOTICE", "Saved " + EmployeeName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
