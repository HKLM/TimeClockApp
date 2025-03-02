#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class PayrollDetailViewModel : BaseViewModel, IQueryAttributable
    {
        protected readonly PayrollService payrollData;

        [ObservableProperty]
        public partial ObservableCollection<TimeCard> TimeCards { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimeCards))]
        public partial TimeSheet SheetTime { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Employee> EmployeeList { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        public partial DateTime StartDate { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PayPeriod))]
        public partial DateTime EndDate { get; set; } = DateTime.Now;
 #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        public string PayPeriod => $"TimeCards for Pay Period {StartDate.ToShortDateString()} to {EndDate.ToShortDateString()}";

        [ObservableProperty]
        public partial double RegTotalHours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalOTHours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalOT2Hours { get; set; } = 0;
        [ObservableProperty]
        public partial double UnPaidRegTotalHours { get; set; } = 0;
        [ObservableProperty]
        public partial double UnPaidTotalOTHours { get; set; } = 0;
        [ObservableProperty]
        public partial double UnPaidTotalOT2Hours { get; set; } = 0;

        [ObservableProperty]
        public partial double TotalWorkHours { get; set; } = 0;
        [ObservableProperty]
        public partial double UnPaidTotalWorkHours { get; set; } = 0;
        [ObservableProperty]
        public partial double TotalGrossPay { get; set; } = 0.00;
        partial void OnTotalGrossPayChanged(double value)
        {
            TotalEstimatedWC = value * WCRate;
        }

        [ObservableProperty]
        public partial double TotalOwedGrossPay { get; set; } = 0.00;

        [ObservableProperty]
        public partial double TotalEstimatedWC { get; set; } = 0.00;

        public double WCRate { get; set; } = 0.00;

        [ObservableProperty]
        public partial bool DisplayLandscapeMode { get; set; } = false;
        partial void OnDisplayLandscapeModeChanged(bool value)
        {
            NotDisplayLandscapeMode = !DisplayLandscapeMode;
        }

        [ObservableProperty]
        public partial bool NotDisplayLandscapeMode { get; set; } = true;

        /// <summary>
        /// Tracks if the filter options are displayed or not
        /// </summary>
        [ObservableProperty]
        public partial bool ShowFilterOptions { get; set; } = false;

        [ObservableProperty]
        public partial Employee? SelectedFilter { get; set; }
        partial void OnSelectedFilterChanged(global::TimeClockApp.Shared.Models.Employee? value)
        {
            TimeCards.Clear();
            Task.Run(async () => await RefreshingCardsAsync(false));
        }
        private int? selectedFilterId;
        private bool LastOnlyUnpaid = false;

        public PayrollDetailViewModel(PayrollService service)
        {
            payrollData = service;
            TimeCards = [];
            EmployeeList = [];
            SelectedFilter = null;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("start"))
            {
                if (DateOnly.TryParseExact(Uri.UnescapeDataString(query["start"].ToString()!), new string[] { "MM/dd/yyyy", "MM-dd-yyyy", "MM.dd.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly sdate))
                    StartDate = new DateTime(sdate.Year, sdate.Month, sdate.Day);
            }
            if (query.ContainsKey("end"))
            {
                if (DateOnly.TryParseExact(Uri.UnescapeDataString(query["end"].ToString()!), new string[] { "MM/dd/yyyy", "MM-dd-yyyy", "MM.dd.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly edate))
                    EndDate = new DateTime(edate.Year, edate.Month, edate.Day);
            }
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { selectedFilterId = i; }
            }
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            StartDate = payrollData.GetStartOfPayPeriod(DateTime.Now);
            PickerMinDate = payrollData.GetAppFirstRunDate();

            WCRate = payrollData.GetWCRate();
            IsAdmin = IntToBool(payrollData.GetConfigInt(9, 0));

            if (EmployeeList.Count == 0 || App.NoticeUserHasChanged)
            {
                Task<List<Employee>> tp = payrollData.GetEmployeeListAsync();
                List<Employee> p = await tp;
                EmployeeList = p.ToObservableCollection();
            }
            if (selectedFilterId > 0)
            {
                try
                {
                    SelectedFilter = payrollData.GetEmployee(selectedFilterId.Value);
                }
                catch (AggregateException ax)
                {
                    HasError = true;
                    TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
                }
                finally
                {
                    selectedFilterId = null;
                }
            }
            await RefreshingCardsAsync(LastOnlyUnpaid);
        }

        [RelayCommand]
        private Task RefreshCardsAsync() => RefreshingCardsAsync(false);

        private async Task RefreshingCardsAsync(bool bOnlyUnpaid = false)
        {
            Loading = true;
            HasError = false;

            try
            {
                LastOnlyUnpaid = bOnlyUnpaid;
                if (DateOnly.FromDateTime(StartDate) > DateOnly.FromDateTime(EndDate))
                {
                    await App.AlertSvc!.ShowAlertAsync("VALIDATION ERROR", "StartDate must be before EndDate");
                    return;
                }
                ResetItems();

                if (SelectedFilter == null)
                    return;

                Task<TimeSheet> s = payrollData.GetPayrollTimeSheetForEmployeeAsync(SelectedFilter.EmployeeId, DateOnly.FromDateTime(StartDate), DateOnly.FromDateTime(EndDate), SelectedFilter.Employee_Name, SheetTime, bOnlyUnpaid);
                SheetTime = await s;
                if (SheetTime?.TimeCards.Any() == true)
                {
                    List<TimeCard> t = SheetTime.TimeCards.ToList();
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
                else if (SheetTime?.TimeCards.Count == 0)
                {
                    TimeCards.Clear();
                }
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            finally
            {
                initDone = true;
                Loading = false;
            }

        }

        [RelayCommand]
        private Task GetAllUnpaidTimeCards()
        {
            return Task.Run(async () => await RefreshingCardsAsync(true));
        }

        [RelayCommand]
        private async Task ReportClockOut(TimeCard card)
        {
            if (card == null)
                return;

            Task<bool> c = payrollData.EmployeeClockOutAsync(card.TimeCardId);
            bool b = await c;
            if (b)
            {
                Task r = RefreshingCardsAsync(LastOnlyUnpaid);
                await r;
            }
        }

        [RelayCommand]
        private async Task MarkPaidAsync(TimeCard card)
        {
            if (card == null)
                return;

            bool m = await payrollData.MarkTimeCardAsPaidAsync(card);
            if (m)
            {
                //TODO fix amount. TotalOwedGrossPay is not for just 1 card
                Task e = payrollData.AddNewExpenseAsync(card.ProjectId, card.PhaseId, TotalOwedGrossPay, string.Empty, card.ProjectName, card.PhaseTitle, 3);
                await e;
                //add entry for WorkersComp amount too
                Task r = RefreshingCardsAsync(LastOnlyUnpaid);
                await r;
            }
        }

        [RelayCommand]
        private async Task MarkAllPaidAsync()
        {
            Loading = true;
            HasError = false;

            List<Task> tasks = [];
            List<Task> taskExpense = [];

            try
            {
                double p = payrollData.GetPayrollInfoForExpense(TimeCards.ToList());
                double wc = p * WCRate;

                int i = TimeCards.Count - 1;
                //default to use the last timecard to file it under that project and phase
                taskExpense.Add(payrollData.AddNewExpenseAsync(TimeCards[i].ProjectId, TimeCards[i].PhaseId, p, string.Empty, TimeCards[i].ProjectName, TimeCards[i].PhaseTitle, 3));
                taskExpense.Add(payrollData.AddNewExpenseAsync(TimeCards[i].ProjectId, TimeCards[i].PhaseId, wc, string.Empty, TimeCards[i].ProjectName, TimeCards[i].PhaseTitle, 4));

                foreach (TimeCard item in TimeCards)
                {
                    if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                    {
                        tasks.Add(payrollData.MarkTimeCardAsPaidAsync(item));
                    }
                }
                await Task.WhenAll(taskExpense);

                if (tasks.Count > 0)
                {
                    await Task.WhenAll(tasks);
                    await RefreshingCardsAsync(LastOnlyUnpaid);
                }
            }
            catch (AggregateException ax)
            {
                HasError = true;
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
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
