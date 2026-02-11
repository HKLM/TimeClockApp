#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ExpenseViewModel : BaseViewModel
    {
        protected readonly ExpenseService dataService;

        [ObservableProperty]
        public partial Expense ExpenseItem { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Expense> ExpenseList { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = new();

        [ObservableProperty]
        public partial Project SelectedProject { get; set; } = new();
        partial void OnSelectedProjectChanged(Project oldValue, Project newValue)
        {
            if (!Loading && oldValue != null && newValue != null && newValue.ProjectId != 0 && newValue.ProjectId != oldValue.ProjectId)
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Refresh().ContinueWith(task =>
                    {
                        if (task.Exception != null)
                        {
                            Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "ExpenseViewModel.OnSelectedNumberOfResultsChanged");
                        }
                    });
                });

        }

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();
        [ObservableProperty]
        public partial Phase SelectedPhase { get; set; } = new();
        //partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase value)
        //{
        //    if (!Loading && value != null && SelectedPhase != null && value.PhaseId != SelectedPhase.PhaseId)
        //        Task.Run(() => dataService.SaveCurrentPhaseAsync(value.PhaseId).ContinueWith(task =>
        //        {
        //            if (task.Exception != null)
        //            {
        //                Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "ExpenseViewModel.OnSelectedPhaseChanging");
        //            }
        //        }));
        //}

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
        public partial bool ShowOnlyProject { get; set; } = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(_LastSelectedNumberOfResults))]
        public partial int SelectedNumberOfResults { get; set; } = 5;
        partial void OnSelectedNumberOfResultsChanged(int value)
        {
            if (!Loading && value != _LastSelectedNumberOfResults)
            {
                _LastSelectedNumberOfResults = value;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Refresh().ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "ExpenseViewModel.OnSelectedNumberOfResultsChanged");
                    }
                });
                });
            }
        }
        private int _LastSelectedNumberOfResults { get; set; } = 5;

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
                List<Project> proList = await dataService.GetProjectsListAsync();
                ProjectList = proList.ToObservableCollection();
                List<Phase> phaseList = await dataService.GetPhaseListAsync();
                PhaseList = phaseList.ToObservableCollection();
                if (SelectedProject == null || SelectedProject.ProjectId < 1)
                    SelectedProject = await dataService.GetCurrentProjectEntityAsync();
                if (SelectedPhase == null || SelectedPhase.PhaseId < 1)
                    SelectedPhase = await dataService.GetCurrentPhaseEntityAsync();

                List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                ExpenseTypeList = x.ToObservableCollection();

                SelectedExpenseType = dataService.GetExpenseType(5);
                SelectedNumberOfResults = 5;

                List<Expense> L = await dataService.GetRecentExpensesListAsync(SelectedProject.ProjectId, ShowRecent, SelectedNumberOfResults);
                ExpenseList = L.ToObservableCollection();
            }
            catch (Exception ex)
            {
                HasError = true;
                Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "TimeCardPageViewModel");
                await App.AlertSvc!.ShowAlertAsync("Exception", $"{ex.Message}\n{ex.InnerException}").ConfigureAwait(false);
            }
            finally
            {
                Loading = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (!Loading)
            {
                try
                {
                    if (!ShowOnlyProject)
                    {
                        List<Expense> L = await dataService.GetAllExpensesListAsync(SelectedNumberOfResults);
                        ExpenseList = L.ToObservableCollection();
                    }
                    else
                    {
                        List<Expense> L = await dataService.GetRecentExpensesListAsync(SelectedProject.ProjectId, ShowRecent, SelectedNumberOfResults);
                        ExpenseList = L.ToObservableCollection();
                    }
                }
                catch (Exception ex)
                {
                    HasError = true;
                    Log.WriteLine(ex.Message + "\n" + ex.InnerException);
                }
            }
        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            if (SelectedProject == null || SelectedProject.ProjectId < 1 || SelectedPhase == null || SelectedExpenseType == null)
                return;

            if (Amount == 0)
            {
                await App.AlertSvc!.ShowAlertAsync("ERROR", "Amount can not be 0").ConfigureAwait(false);
                return;
            }

            try
            {
                string expenseNewMemo = Memo.Trim();
                if (dataService.AddNewExpense(SelectedProject.ProjectId, SelectedPhase.PhaseId, Amount, expenseNewMemo, SelectedProject.Name, SelectedPhase.PhaseTitle, SelectedExpenseType.ExpenseTypeId, SelectedExpenseType.CategoryName))
                {
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Saved").ConfigureAwait(false);
                    Amount = 0;
                    await Refresh();
                }
                else
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Failed to save Expense").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                HasError = true;
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
                    //if (dataService.ArchiveExpense(ExpenseList))
                    //    ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
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
        private async Task ToggleShowArchived(bool ToggledValue)
        {
            ShowArchived = ToggledValue;
            await Refresh();
        }

        [RelayCommand]
        private async Task ToggleShowOnlyProject(bool ToggledValue)
        {
            ShowOnlyProject = ToggledValue;
            await Refresh();
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
