using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class TeamEmployeesViewModel : TimeStampViewModel
    {
        protected UserManagerService employeeService;

        [ObservableProperty]
        private ObservableCollection<Employee> employee_List = new();

        public TeamEmployeesViewModel()
        {
            employeeService = new();
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
            if (IsBusy)
                return;

            IsBusy = true;

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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SetTeamNotActive(Employee? employee)
        {
            if (IsBusy)
                return;

            IsBusy = true;

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
