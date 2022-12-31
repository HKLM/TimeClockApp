using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class ReportWeekViewModel : TimeStampViewModel
    {
        protected readonly ReportDataService reportData;
        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCards = new();
        [ObservableProperty]
        private ObservableCollection<Wages> wagesList = new();
        [ObservableProperty]
        private ObservableCollection<Wages> owedWagesList = new();

        [ObservableProperty]
        private Employee selectedFilter;
        partial void OnSelectedFilterChanged(global::TimeClockApp.Models.Employee value)
        {
            Refresh_Cards();
        }

        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = new();

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject;

        [ObservableProperty]
        private double regTotalHours = 0;
        [ObservableProperty]
        private double totalOTHours = 0;
        [ObservableProperty]
        private double totalOT2Hours = 0;
        [ObservableProperty]
        private double regTotalPay = 0;
        [ObservableProperty]
        private double totalOTPay = 0;
        [ObservableProperty]
        private double totalOT2Pay = 0;

        [ObservableProperty]
        private double totalWorkHours = 0;
        [ObservableProperty]
        private double totalPaidWorkHours = 0;
        [ObservableProperty]
        private double totalGrossPay = 0.00;

        [ObservableProperty]
        private double totalOwedGrossPay = 0.00;
        [ObservableProperty]
        private DateTime startDate = DateTime.Now;
        [ObservableProperty]
        private DateTime endDate = DateTime.Now;

        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion

        [ObservableProperty]
        private bool displayLandscapeMode = false;
        partial void OnDisplayLandscapeModeChanged(bool value)
        {
            NotDisplayLandscapeMode = !DisplayLandscapeMode;
        }

        [ObservableProperty]
        private bool notDisplayLandscapeMode = true;
        /// <summary>
        /// Tracks if the filter options are displayed or not
        /// </summary>
        [ObservableProperty]
        private bool showFilterOptions = false;

        public ReportWeekViewModel()
        {
            reportData = new();
            ShowFilterOptions = false;
            EndDate = DateTime.Now;
            StartDate = reportData.GetStartOfPayPeriod(DateTime.Now);
            PickerMinDate = reportData.GetAppFirstRunDate();
        }

        public async Task OnAppearingAsync()
        {
            LoadProjects();
            await RefreshEmployeesAsync();
            await RefreshCardsAsync();
        }

        private async Task RefreshEmployeesAsync()
        {
            EmployeeList ??= new();
            if (EmployeeList.Any() == false || App.NoticeUserHasChanged == true)
                EmployeeList = await reportData.GetAllEmployeesAsync(false);
        }

        private void Refresh_Cards()
        {
            ResetWageItems();
            if (SelectedFilter != null && SelectedFilter.EmployeeId > 0)
            {
                TimeCards.Clear();
                WagesList.Clear();
                if (ShowFilterOptions && SelectedProject != null)
                    (TimeCards, WagesList, OwedWagesList) = reportData.GetReportTimeCardsForEmployee(SelectedFilter.EmployeeId, SelectedProject.ProjectId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));
                else
                    (TimeCards, WagesList, OwedWagesList) = reportData.GetReportTimeCardsForEmployee(SelectedFilter.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));
                if (WagesList != null && WagesList.Any())
                {
                    (RegTotalHours, TotalOTHours, TotalOT2Hours, TotalWorkHours) = reportData.GetHours(WagesList.ToList());
                    (RegTotalPay, TotalOTPay, TotalOT2Pay, TotalGrossPay) = reportData.GetPay(WagesList.ToList());
                    (_, _, _, TotalOwedGrossPay) = reportData.GetPay(OwedWagesList.ToList());
                }
            }
        }

        [RelayCommand]
        private Task RefreshCardsAsync()
        {
            return Task.Run(async () =>
            {
                ResetWageItems();
                if (SelectedFilter != null && SelectedFilter.EmployeeId > 0)
                {
                    TimeCards.Clear();
                    WagesList.Clear();
                    if (ShowFilterOptions && SelectedProject != null)
                        (TimeCards, WagesList, OwedWagesList) = await reportData.GetReportTimeCardsForEmployeeAsync(SelectedFilter.EmployeeId, SelectedProject.ProjectId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));
                    else
                        (TimeCards, WagesList, OwedWagesList) = await reportData.GetReportTimeCardsForEmployeeAsync(SelectedFilter.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate));
                    if (WagesList != null && WagesList.Any())
                    {
                        (RegTotalHours, TotalOTHours, TotalOT2Hours, TotalWorkHours) = reportData.GetHours(WagesList.ToList());
                        (RegTotalPay, TotalOTPay, TotalOT2Pay, TotalGrossPay) = reportData.GetPay(WagesList.ToList());
                        (_, _, _, TotalOwedGrossPay) = reportData.GetPay(OwedWagesList.ToList());
                    }
                }
            });
        }

        private void ResetWageItems()
        {
            RegTotalHours = 0;
            TotalOTHours = 0;
            TotalOT2Hours = 0;
            TotalWorkHours = 0;
            RegTotalPay = 0;
            TotalOTPay = 0;
            TotalOT2Pay = 0;
            TotalGrossPay = 0;
            TotalOwedGrossPay = 0;
        }

        private void LoadProjects()
        {
            ProjectList ??= new();
            if (ProjectList.Any() == false || App.NoticeProjectHasChanged == true)
                ProjectList = reportData.GetAllProjectsList(true);
        }

        [RelayCommand]
        private async Task LoadTimeCardsAsync()
        {
            if (IsBusy)
                return;
            if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
            {
                await App.AlertSvc.ShowAlertAsync("VALIDATION ERROR", "StartDate must be before EndDate");
                return;
            }
            if (SelectedFilter == null || SelectedFilter.EmployeeId == 0)
            {
                await App.AlertSvc.ShowAlertAsync("VALIDATION ERROR", "You must select a employee first");
                return;
            }
            IsBusy = true;

            try
            {
                await RefreshCardsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
                //throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MarkPaidAsync(TimeCard card)
        {
            if (IsBusy || card == null)
                return;

            IsBusy = true;

            try
            {
                if (await reportData.MarkTimeCardAsPaidAsync(card))
                    await RefreshCardsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
                //throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MarkAllPaidAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                foreach (TimeCard item in TimeCards)
                {
                    if (await reportData.MarkTimeCardAsPaidAsync(item))
                        System.Diagnostics.Debug.WriteLine("Marked card as paid");
                }
                await RefreshCardsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);

                //throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GetAllUnpaidTimeCards()
        {
            if (IsBusy)
                return;

            if (SelectedFilter != null && SelectedFilter.EmployeeId > 0)
            {
                IsBusy = true;

                try
                {
                    if (TimeCards.Any())
                        TimeCards.Clear();

                    (TimeCards, TotalWorkHours) = await reportData.GetAllUnpaidTimeCardsForEmployeeAsync(SelectedFilter.EmployeeId);

                    TotalPaidWorkHours = 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                    await App.AlertSvc.ShowAlertAsync("EXCEPTION", ex.Message + "\n" + ex.InnerException);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        private async Task ReportClockOut(TimeCard card)
        {
            if (IsBusy || card == null)
                return;

            IsBusy = true;

            try
            {
                if (await reportData.EmployeeClockOutAsync(card))
                    await RefreshCardsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("EXCEPTION", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ChangeDisplayLandscapeMode(bool isLandscapeMode)
        {
            DisplayLandscapeMode = isLandscapeMode;
        }

        [RelayCommand]
        private void ToggleFilterOptions()
        {
            ShowFilterOptions = !ShowFilterOptions;
            if (!ShowFilterOptions)
            {
                SelectedProject = null;
                EndDate = DateTime.Now;
                StartDate = reportData.GetStartOfPayPeriod(DateTime.Now);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
