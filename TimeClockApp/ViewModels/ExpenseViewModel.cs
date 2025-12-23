#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ExpenseViewModel : BaseViewModel
    {
        protected readonly ExpenseService dataService;

        [ObservableProperty]
        public partial Expense ExpenseItem { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Expense> ExpenseList { get; set; } = new ();

        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = new();

        [ObservableProperty]
        public partial Project SelectedProject { get; set; } = new();
        partial void OnSelectedProjectChanged(global::TimeClockApp.Shared.Models.Project value)
        {
            if (value != null && dataService != null && value.ProjectId != 0)
                ExpenseList = dataService.GetAllExpenses(value.ProjectId);
        }

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();
        [ObservableProperty]
        public partial Phase SelectedPhase { get; set; } = new();
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase value)
        {
            if (value != null)
            {
                if (!App.CurrentPhaseId.HasValue || App.CurrentPhaseId.HasValue && value.PhaseId != App.CurrentPhaseId.Value)
                    Task.Run(async () => await dataService.SaveCurrentPhaseAsync(value.PhaseId).ConfigureAwait(false));
            }
        }

        [ObservableProperty]
        public partial ObservableCollection<ExpenseType> ExpenseTypeList { get; set; } = new();
        [ObservableProperty]
        public partial ExpenseType SelectedExpenseType { get; set; } = new();

        [ObservableProperty]
        public partial DateOnly ExpenseDate { get; set; }
        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;

        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion

        [ObservableProperty]
        public partial Double Amount { get; set; }
        [ObservableProperty]
        public partial bool IsRefreshingList { get; set; }
        [ObservableProperty]
        public partial bool ShowArchived { get; set; }
        private bool ShowRecent => !ShowArchived;

        [ObservableProperty]
        public partial bool ShowOnlyProject { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LastSelectedNumberOfResults))]
        public partial int SelectedNumberOfResults { get; set; } = 5;
        partial void OnSelectedNumberOfResultsChanged(int value)
        {
            if (value != LastSelectedNumberOfResults)
            {
                LastSelectedNumberOfResults = value;
                Refresh();
            }
        }

        private int LastSelectedNumberOfResults { get; set; } = 5;

        [ObservableProperty]
        public partial string Memo { get; set; } = string.Empty;

        /// <summary>
        /// The ToolBar Icon toggles displaying the OptionsBox for the page
        /// </summary>
        [ObservableProperty]
        public partial bool OptionsBoxVisible { get; set; } = false;

        public ExpenseViewModel(ExpenseService service)
        {
            dataService = service;
            ShowArchived = false;
            PickerMinDate = dataService.GetAppFirstRunDate();
            pickerMaxDate = DateTime.Now;
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            Loading = true;
            HasError = false;

            try
            {
                ExpenseDate = DateOnly.FromDateTime(DateTime.Now);
                //Only get data from DB once, unless it has been notified that it has changed
                if (ProjectList.Count == 0 || App.NoticeProjectHasChanged)
                {
                    Task<List<Project>> tp = dataService.GetProjectsListAsync();
                    List<Project> p = await tp;
                    ProjectList = p.ToObservableCollection();
                }

                if (PhaseList.Count == 0 || App.NoticePhaseHasChanged)
                {
                    Task<List<Phase>> tph = dataService.GetPhaseListAsync();
                    List<Phase> ph = await tph;
                    PhaseList = ph.ToObservableCollection();
                }

                if (App.NoticeProjectHasChanged || SelectedProject == null || SelectedProject.ProjectId == 0)
                {
                    var tcp = dataService.GetCurrentProjectEntityAsync();
                    Project cp = await tcp;
                    SelectedProject = cp;
                }

                if (SelectedPhase == null || SelectedPhase.PhaseId == 0 || App.NoticePhaseHasChanged)
                {
                    var tcph = dataService.GetCurrentPhaseEntityAsync();
                    Phase cph = await tcph;
                    SelectedPhase = cph;
                }

                List<ExpenseType> x = dataService.GetExpenseTypeList();
                ExpenseTypeList = x.ToObservableCollection();

                SelectedExpenseType = dataService.GetExpenseType(5);
                SelectedNumberOfResults = 5;
            }
            catch (AggregateException ax)
            {
                HasError = true;
                string z = FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "TimeCardPageViewModel");
                await App.AlertSvc!.ShowAlertAsync("Exception", z);
            }
            catch (Exception ex)
            {
                HasError = true;
                Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "TimeCardPageViewModel");
                await App.AlertSvc!.ShowAlertAsync("Exception", $"{ex.Message}\n{ex.InnerException}");
            }
            finally
            {
                Loading = false;
            }
        }

        private void Refresh()
        {
            try
            {
                if (ShowOnlyProject && SelectedProject?.ProjectId > 0 || !ShowOnlyProject && ShowRecent)
                {
                    Task.Run(async () =>
                    {
                        List<Expense> L = await dataService.GetExpenseListAsync(SelectedProject?.ProjectId, ShowRecent, false, SelectedNumberOfResults);
                        ExpenseList = L.ToObservableCollection();
                    });
                }
                else if (!ShowOnlyProject)
                {
                    Task.Run(async () =>
                    {
                        List<Expense> L = await dataService.ExpensesListAsync(SelectedNumberOfResults);
                        ExpenseList = L.ToObservableCollection();
                    });
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            if (SelectedProject == null || SelectedProject.ProjectId < 1 || SelectedPhase == null || SelectedExpenseType == null)
                return;

            if (Amount == 0)
            {
                await App.AlertSvc!.ShowAlertAsync("ERROR", "Amount can not be 0");
                return;
            }

            try
            {
                string expenseNewMemo = Memo.Trim();
                if (dataService.AddNewExpense(SelectedProject.ProjectId, SelectedPhase.PhaseId, Amount, expenseNewMemo, SelectedProject.Name, SelectedPhase.PhaseTitle, SelectedExpenseType.ExpenseTypeId, SelectedExpenseType.CategoryName))
                {
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Saved");
                    Amount = 0;
                    Refresh();
                }
                else
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Failed to save Expense");
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
        private void ArchiveExpenseList()
        {
            IsRefreshingList = true;
            try
            {
                if (ExpenseList?.Any() == true)
                {
                    if (dataService.ArchiveExpense(ExpenseList))
                        ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
                }
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
                IsRefreshingList = false;
            }
            finally
            {
                IsRefreshingList = false;
            }
        }

        [RelayCommand]
        private void ToggleShowArchived(bool ToggledValue)
        {
            ShowArchived = ToggledValue;
            Refresh();
        }

        [RelayCommand]
        private void ToggleShowOnlyProject(bool ToggledValue)
        {
            ShowOnlyProject = ToggledValue;
            Refresh();
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        [RelayCommand]
        private void OnToggleOptionsBoxVisible()
        {
            OptionsBoxVisible = !OptionsBoxVisible;
        }
    }
}
