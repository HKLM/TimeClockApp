using TimeClockApp.Shared.Interfaces;
using TimeClockApp.Utilities;

#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class InvoiceViewModel(InvoiceService service, ISharedService sharedService) : BaseViewModel
    {
        protected readonly InvoiceService invoiceData = service;
        protected readonly ISharedService _SharedService = sharedService;

        private List<TimeSheet> TimeSheetList = new();

        [ObservableProperty]
        public partial bool UseDateFilter { get; set; } = true;
        [ObservableProperty]
        public partial bool AllExpenseTypes { get; set; }
        [ObservableProperty]
        public partial DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
        [ObservableProperty]
        public partial DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        //For paid timecards active during the invoice period and already have a expense entry for the WC
        private double TotalPaidGrossPay = 0.00;
        //For unpaid timecards active during the invoice period. This will be used to estimate the WC cost.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetTotalLaborBurden))]
        public partial double TotalUnpaidGrossPay { get; set; } = 0.00;
        partial void OnTotalUnpaidGrossPayChanged(double value)
        {
            TotalEstimatedWC = value * WCRate;
        }

        private double TotalEstimatedWC = 0.00;
        private double TotalPaidWC = 0.00;

        [ObservableProperty]
        public partial double WCRate { get; set; } = 0.00;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetTotalLaborBurden))]
        [NotifyPropertyChangedFor(nameof(GetTotalInvoice))]
        public partial double TotalLaborBurden { get; set; } = 0.00;
        public double GetTotalLaborBurden => TotalPaidGrossPay + TotalUnpaidGrossPay;

        // UI display value
        [ObservableProperty]
        public partial double TotalOverhead { get; set; } = 0.00;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetTotalOverhead))]
        public partial double OverheadRate { get; set; } = 0.00;
        public double GetTotalOverhead
        {
            get => OverheadRate * (TotalEstimatedWC + TotalPaidWC + GetTotalOtherOverhead);
        }

        // UI display value
        [ObservableProperty]
        public partial double TotalProfit { get; set; } = 0.00;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetTotalProfit))]
        public partial double ProfitRate { get; set; } = 0.00;
        public double GetTotalProfit
        {
            get => ProfitRate * (TotalExpenses + GetTotalLaborBurden + GetTotalOverhead + TotalOtherFee);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetTotalInvoice))]
        public partial double TotalInvoice { get; set; } = 0.00;

        public double GetTotalInvoice
        {
            get { return TotalExpenses + GetTotalLaborBurden + GetTotalOverhead + GetTotalProfit + TotalOtherFee; }
        }
#region TODO

        //OtherFee (markup) for other fee, tax, etc
        [ObservableProperty]
        public partial double TotalOtherFee { get; set; } = 0.00;

        //display name for OtherFee
        [ObservableProperty]
        public partial string? TotalOtherFeeName { get; set; }

        //OtherFee for other fee, tax, etc
        [ObservableProperty]
        public partial double? TotalOtherOverhead { get; set; }
        //TODO
        public double GetTotalOtherOverhead => TotalOtherOverhead ?? 0;

        //display name for OtherFee
        [ObservableProperty]
        public partial string? TotalOtherOverheadName { get; set; }
#endregion

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GetTotalInvoice))]
        public partial double TotalExpenses { get; set; } = 0.00;

        [ObservableProperty]
        public partial bool UseProjectFilter { get; set; } = true;
        partial void OnUseProjectFilterChanged(bool value)
        {
            if (value == true)
                ProjectList = invoiceData.GetProjectsList();
        }
        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = new();
        [ObservableProperty]
        public partial Project? SelectedProject { get; set; } = null;

        [ObservableProperty]
        public partial bool UsePhaseFilter { get; set; } = false;
        partial void OnUsePhaseFilterChanged(bool value)
        {
            if (value == true)
                PhaseList = invoiceData.GetPhaseList();
        }
        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();
        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;

        public List<TimeCard> CardList = new();
        public List<Expense> ExpenseList = new();

        public async Task OnAppearing()
        {
			Loading = true;
			HasError = false;
            try
            {
                PickerMinDate = invoiceData.GetAppFirstRunDate();
                WCRate = await invoiceData.GetWCRateAsync();
                ProfitRate = await invoiceData.GetProfitRateAsync();
                TotalOtherOverhead = await invoiceData.GetOverheadRateAsync();
                List<Project> p = await invoiceData.GetProjectsListAsync();
                ProjectList = p.ToObservableCollection();
                SelectedProject = await invoiceData.GetCurrentProjectEntityAsync();
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "InvoiceViewModel.OnAppearing");
            }
			finally
			{
				Loading = false;
			}
		}

        [RelayCommand]
        private async Task MakeInvoice()
        {
            if (Loading) return;

            Loading = true;
            HasError = false;

            try
            {
                if (SelectedProject == null)
                {
                    await invoiceData.ShowPopupErrorAsync("Select a Project to continue.", "NOTICE").ConfigureAwait(false);
                    return;
                }
                ResetItems();
                if (SelectedPhase == null)
                    UsePhaseFilter = false;
                List<TimeSheet> timeSheetList = await invoiceData.RunInvoiceReportAsync(UsePhaseFilter, SelectedProject, SelectedPhase, StartDate, EndDate); 
                TimeSheetList = timeSheetList;
                foreach (TimeSheet sheet in timeSheetList)
                {
                    TotalUnpaidGrossPay += sheet.TotalGrossPay;
                    CardList.AddRange(sheet.TimeCards);
                }
                //FIX
                TotalExpenses = await invoiceData.GetProjectExpensesAmountAsync(SelectedProject.ProjectId, StartDate, EndDate, AllExpenseTypes);
                TotalLaborBurden = GetTotalLaborBurden;
                TotalOverhead = GetTotalOverhead;
                TotalProfit = GetTotalProfit;
                TotalInvoice = GetTotalInvoice;
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "InvoiceViewModel.MakeInvoice");
            }
            finally
            {
                Loading = false;
            }
        }

        //TODO - Save invoice as a PDF file
        /*
                [RelayCommand]
                private async Task MakePDFInvoice()
                {
                    await Task.Run(() => MakePDF());
                }

        */
        [RelayCommand]
        private async Task GoToInvoiceDetailTimecards()
        {
            if (App.Current != null && CardList != null && CardList.Any())
            {
                try
                {
                    InvoiceDetailTimecardsViewModel viewModel = ServiceHelper.GetService<InvoiceDetailTimecardsViewModel>();
                    _SharedService.Add<List<TimeCard>>("CardList", CardList);
                    await MainThread.InvokeOnMainThreadAsync(() => App.Current!.Windows[0].Page!.Navigation.PushAsync(new InvoiceDetailTimecards(viewModel), true));
                }
                catch (Exception ex)
                {
                    HasError = true;
                    Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "InvoiceViewModel");
                    await App.AlertSvc!.ShowAlertAsync("Exception", $"{ex.Message}\n{ex.InnerException}").ConfigureAwait(false);
                }
            }
            else
            {
                await App.AlertSvc!.ShowAlertAsync("INFO", "No data found.").ConfigureAwait(false);
            }
        }

        [RelayCommand]
        private async Task GoToInvoiceDetailExpenses()
        {
            if (SelectedProject != null && App.Current != null)
            {
                try
                {
                    ExpenseList = await invoiceData.GetProjectExpensesListAsync(SelectedProject.ProjectId, StartDate, EndDate, AllExpenseTypes);
                    if (ExpenseList is null || !ExpenseList.Any())
                    {
                        await App.AlertSvc!.ShowAlertAsync("INFO", "No data to view.").ConfigureAwait(false);
                        return;
                    }
                    InvoiceDetailExpensesViewModel viewModel = ServiceHelper.GetService<InvoiceDetailExpensesViewModel>();
                    _SharedService.Add<List<Expense>>("ExpenseList", ExpenseList);
                    await MainThread.InvokeOnMainThreadAsync(() => App.Current!.Windows[0].Page!.Navigation.PushAsync(new InvoiceDetailExpenses(viewModel), true));
                }
                catch (AggregateException ax)
                {
                    HasError = true;
                    string message = TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "InvoiceViewModel");
                    await App.AlertSvc!.ShowAlertAsync("Exception", message).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    HasError = true;
                    Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "InvoiceViewModel");
                    await App.AlertSvc!.ShowAlertAsync("Exception", $"{ex.Message}\n{ex.InnerException}").ConfigureAwait(false);
                }
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        private void ResetItems()
        {
            TotalExpenses = 0;
            TotalOverhead = 0;
            TotalInvoice = 0;
            TotalUnpaidGrossPay = 0;
            TotalPaidGrossPay = 0;
            TotalPaidWC = 0;

            CardList.Clear();
        }
    }
}
