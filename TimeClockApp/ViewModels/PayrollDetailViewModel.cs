﻿using CommunityToolkit.Maui.Core.Extensions;

namespace TimeClockApp.ViewModels
{
    [QueryProperty("StartString", "start")]
    [QueryProperty("EndString", "end")]
    [QueryProperty("IdString", "id")]
    public partial class PayrollDetailViewModel(PayrollService service) : TimeStampViewModel, IQueryAttributable
    {
        protected readonly PayrollService payrollData = service;
        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCards = new();
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimeCards))]
        private TimeSheet sheetTime = new();

        [ObservableProperty]
        private ObservableCollection<Employee> employeeList = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        private DateTime startDate;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        private DateTime endDate = DateTime.Now;
 #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        public string PayPeriod => $"TimeCards for Pay Period {StartDate.ToShortDateString()} to {EndDate.ToShortDateString()}";

        [ObservableProperty]
        private double regTotalHours = 0;
        [ObservableProperty]
        private double totalOTHours = 0;
        [ObservableProperty]
        private double totalOT2Hours = 0;
        [ObservableProperty]
        private double unPaidRegTotalHours = 0;
        [ObservableProperty]
        private double unPaidTotalOTHours = 0;
        [ObservableProperty]
        private double unPaidTotalOT2Hours = 0;

        [ObservableProperty]
        private double totalWorkHours = 0;
        [ObservableProperty]
        private double unPaidTotalWorkHours = 0;
        [ObservableProperty]
        private double totalGrossPay = 0.00;
        [ObservableProperty]
        private double totalOwedGrossPay = 0.00;

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

        [ObservableProperty]
        private Employee selectedFilter;
        partial void OnSelectedFilterChanged(global::TimeClockApp.Shared.Models.Employee value)
        {
            TimeCards.Clear();
            Refresh_Cards();
        }
        private int? selectedFilterId;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("start"))
            {
                if (DateOnly.TryParseExact(Uri.UnescapeDataString(query["start"].ToString()), new string[] { "MM/dd/yyyy", "MM-dd-yyyy", "MM.dd.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly sdate))
                    StartDate = new DateTime(sdate.Year, sdate.Month, sdate.Day);
            }
            if (query.ContainsKey("end"))
            {
                if (DateOnly.TryParseExact(Uri.UnescapeDataString(query["end"].ToString()), new string[] { "MM/dd/yyyy", "MM-dd-yyyy", "MM.dd.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly edate))
                    EndDate = new DateTime(edate.Year, edate.Month, edate.Day);
            }
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { selectedFilterId = i; }
            }
        }

        public async Task OnAppearing()
        {
            await RefreshCardsAsync();
        }

        [RelayCommand]
        private async Task InitAsync()
        {
            Loading = true;
            HasError = false;

            StartDate = payrollData.GetStartOfPayPeriod(DateTime.Now);
            PickerMinDate = payrollData.GetAppFirstRunDate();
            try
            {
                Task r = RefreshEmployeesAsync();
                await r;
            }
            catch
            {
                HasError = true;
            }
            finally
            {
                initDone = true;
                Loading = false;
            }
        }

        private async Task RefreshEmployeesAsync()
        {
            EmployeeList ??= [];
            if (EmployeeList.Count > 0)
                EmployeeList.Clear();

            Task<ObservableCollection<Employee>> g = payrollData.GetEmployeesAsync();
            EmployeeList = await g;
            if (selectedFilterId.HasValue && selectedFilterId.Value > 0)
            {
                try
                {
                    SelectedFilter = payrollData.GetEmployee(selectedFilterId.Value);
                }
                catch
                {
                    HasError = true;
                    //throw;
                }
                finally
                {
                    selectedFilterId = null;
                }
            }
        }

        [RelayCommand]
        private async Task RefreshingCardsAsync() => await RefreshCardsAsync(true);

        private async Task RefreshCardsAsync(bool bShowPaid = true)
        {
            if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
            {
                App.AlertSvc.ShowAlert("VALIDATION ERROR", "StartDate must be before EndDate");
                return;
            }
            ResetItems();

            if (SelectedFilter == null)
                return;

            SheetTime = await payrollData.GetPayrollTimeSheetForEmployeeAsync(SelectedFilter.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate), SelectedFilter.Employee_Name, SheetTime, bShowPaid);
            if (SheetTime?.TimeCards.Any() == true)
            {
                IList<TimeCard> t = SheetTime.TimeCards;
                TimeCards = t.ToObservableCollection();

                RegTotalHours = SheetTime.RegTotalHours;
                TotalOTHours = SheetTime.TotalOTHours;
                TotalOT2Hours = SheetTime.TotalOT2Hours;
                UnPaidRegTotalHours = SheetTime.UnPaidRegTotalHours;
                UnPaidTotalOTHours = SheetTime.UnPaidTotalOTHours;
                UnPaidTotalOT2Hours = SheetTime.UnPaidTotalOT2Hours;
                TotalWorkHours = SheetTime.TotalWorkHours;
                UnPaidTotalWorkHours = SheetTime.UnPaidTotalWorkHours;
                TotalGrossPay = SheetTime.TotalGrossPay;
                TotalOwedGrossPay = SheetTime.TotalOwedGrossPay;
            }
        }

        private void Refresh_Cards(bool bShowPaid = true)
        {
            if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
            {
                App.AlertSvc.ShowAlert("VALIDATION ERROR", "StartDate must be before EndDate");
                return;
            }
            ResetItems();

            if (SelectedFilter == null)
                return;

            SheetTime = payrollData.GetPayrollTimeSheetForEmployee(SelectedFilter.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate), SelectedFilter.Employee_Name, SheetTime, bShowPaid);
            if (SheetTime?.TimeCards.Any() == true)
            {
                IList<TimeCard> t = SheetTime.TimeCards;
                TimeCards = t.ToObservableCollection();

                RegTotalHours = SheetTime.RegTotalHours;
                TotalOTHours = SheetTime.TotalOTHours;
                TotalOT2Hours = SheetTime.TotalOT2Hours;
                UnPaidRegTotalHours = SheetTime.UnPaidRegTotalHours;
                UnPaidTotalOTHours = SheetTime.UnPaidTotalOTHours;
                UnPaidTotalOT2Hours = SheetTime.UnPaidTotalOT2Hours;
                TotalWorkHours = SheetTime.TotalWorkHours;
                UnPaidTotalWorkHours = SheetTime.UnPaidTotalWorkHours;
                TotalGrossPay = SheetTime.TotalGrossPay;
                TotalOwedGrossPay = SheetTime.TotalOwedGrossPay;
            }
        }

        [RelayCommand]
        private async Task GetAllUnpaidTimeCards()
        {
            await Task.Run(async () => await RefreshCardsAsync(false));
        }

        [RelayCommand]
        private async Task ReportClockOut(TimeCard card)
        {
            if (card == null)
                return;

            Task<bool> b = payrollData.EmployeeClockOutAsync(card);
            if (await b)
            {
                Task refreshed = RefreshCardsAsync();
                await refreshed;
            }
        }

        [RelayCommand]
        private async Task MarkPaidAsync(TimeCard card)
        {
            if (card == null)
                return;

            Task<bool> m = payrollData.MarkTimeCardAsPaidAsync(card);
            if (await m)
            {
                Task refreshed = RefreshCardsAsync();
                await refreshed;
            }
        }

        [RelayCommand]
        private async Task MarkAllPaidAsync()
        {
            Loading = true;
            HasError = false;

            List<Task> tasks = [];
            try
            {
                foreach (TimeCard item in TimeCards)
                {
                    if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                        tasks.Add(payrollData.MarkTimeCardAsPaidAsync(item));
                }
                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                    Task refreshed = RefreshCardsAsync();
                    await refreshed;
                }
            }
            catch
            {
                HasError = true;
            }
            finally
            {
                initDone = true;
                Loading = false;
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
                EndDate = DateTime.Now;
                StartDate = payrollData.GetStartOfPayPeriod(DateTime.Now);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        private void ResetItems()
        {
            //TimeCards.Clear();

            RegTotalHours = 0;
            TotalOTHours = 0;
            TotalOT2Hours = 0;
            TotalWorkHours = 0;
            UnPaidTotalWorkHours = 0;
            UnPaidRegTotalHours = 0;
            UnPaidTotalOTHours = 0;
            UnPaidTotalOT2Hours = 0;
            TotalGrossPay = 0;
            TotalOwedGrossPay = 0;
        }
    }
}
