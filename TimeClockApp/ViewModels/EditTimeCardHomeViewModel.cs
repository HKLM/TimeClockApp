using System;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class EditTimeCardHomeViewModel : TimeStampViewModel
    {
        protected EditTimeCardService cardService;
        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCardsHome = new();
        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = new();

        [ObservableProperty]
        private Employee selectedFilter;
        partial void OnSelectedFilterChanged(Employee value)
        {
            GetTimeCardList();
        }

        [ObservableProperty]
        private bool refreshingData;

        [ObservableProperty]
        private bool showPaid = false;
        partial void OnShowPaidChanged(bool value)
        {
            GetTimeCardList();
        }

        public EditTimeCardHomeViewModel()
        {
            cardService = new();
            EmployeeList = cardService.GetAllEmployees();
        }

        public void OnAppearing()
        {
            GetTimeCardList();
        }

        [RelayCommand]
        private void LoadTimeCards()
        {
            if (IsBusy)
                return;
            RefreshingData = true;
            IsBusy = true;

            try
            {
                if (SelectedFilter != null && SelectedFilter.EmployeeId != 0)
                {
                    if (TimeCardsHome.Any())
                        TimeCardsHome.Clear();
                    TimeCardsHome = cardService.GetTimeCardsForEmployee(SelectedFilter.EmployeeId, ShowPaid);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                RefreshingData = false;
            }
            finally
            {
                IsBusy = false;
            }
            RefreshingData = false;
        }

        private void GetTimeCardList()
        {
            if (SelectedFilter != null && SelectedFilter.EmployeeId != 0)
            {
                if (TimeCardsHome.Any())
                    TimeCardsHome.Clear();

                try
                {
                    TimeCardsHome = cardService.GetTimeCardsForEmployee(SelectedFilter.EmployeeId, ShowPaid);
                }
                catch (Exception ex)
                {
                    App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
                }
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
