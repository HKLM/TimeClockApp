namespace TimeClockApp.ViewModels
{
    [QueryProperty("IdExpense", "id")]
    public partial class EditExpensePageViewModel : TimeStampViewModel, IDisposable
    {
        protected ExpenseService dataService;
        public string IdExpense
        {
            set
            {
                if (value != null && int.TryParse(Uri.UnescapeDataString(value), out int i))
                {
                    if (i > 0)
                        ExpenseId = i;
                }
            }
        }
        [ObservableProperty]
        private int expenseId = 0;
        partial void OnExpenseIdChanged(int value)
        {
            Refresh();
        }

        [ObservableProperty]
        private Expense expenseItem = new();

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject = new();

        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = new();
        [ObservableProperty]
        private Phase selectedPhase = new();

        [ObservableProperty]
        private ExpenseType category = ExpenseType.Materials;

        public IReadOnlyList<string> AllCategory { get; } = Enum.GetNames(typeof(ExpenseType));

        [ObservableProperty]
        private DateOnly expenseDate;
        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion
        [ObservableProperty]
        private Double amount;
        [ObservableProperty]
        private string memo;
        [ObservableProperty]
        private bool isRefreshingList;

        public EditExpensePageViewModel()
        {
            dataService = new();
            pickerMaxDate = DateTime.Now;
        }

        public void OnAppearing()
        {
            PickerMinDate = dataService.GetAppFirstRunDate();
            ExpenseDate = DateOnly.FromDateTime(DateTime.Now);
            Refresh();
        }

        private void Refresh()
        {
            RefreshProjectPhases();
            if (ExpenseId != 0)
            {
                ExpenseItem = dataService.GetExpense(ExpenseId);

                Amount = ExpenseItem.Amount;
                Memo = ExpenseItem.Memo;
                ExpenseDate = ExpenseItem.ExpenseDate;
                Category = ExpenseItem.Category;
                SelectedProject = ExpenseItem.Project;
                SelectedPhase = ExpenseItem.Phase;
            }
        }

        private void RefreshProjectPhases()
        {
            //Only get data from DB once, unless it has been notified that it has changed
            ProjectList ??= new();
            if (ProjectList.Any() == false || App.NoticeProjectHasChanged == true)
                ProjectList = dataService.GetProjectsList();

            PhaseList ??= new();
            if (PhaseList.Any() == false || App.NoticePhaseHasChanged == true)
                PhaseList = dataService.GetPhaseList();
        }

        [RelayCommand]
        private async Task EditExpenseAsync()
        {
            if (ExpenseItem == null || ExpenseItem.ExpenseId == 0)
                return;

            try
            {
                ExpenseItem.Amount = Amount;
                ExpenseItem.Memo = Memo;
                ExpenseItem.ExpenseDate = ExpenseDate;
                ExpenseItem.Category = Category;
                ExpenseItem.ProjectId = SelectedProject.ProjectId;
                ExpenseItem.PhaseId = SelectedPhase.PhaseId;
                ExpenseItem.ExpenseProject = SelectedProject.Name;
                ExpenseItem.ExpensePhase = SelectedPhase.PhaseTitle;

                if (dataService.UpdateExpense(ExpenseItem))
                {
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Saved");
                    Refresh();
                }
                else
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to save Expense");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("Exception", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DelExpenseAsync()
        {
            if (ExpenseItem == null || ExpenseItem.ExpenseId == 0)
                return;

            try
            {
                if (await App.AlertSvc.ShowConfirmationAsync("CONFIRMATION", "Are you sure you want to Delete this expense?"))
                {
                    if (await dataService.DeleteExpense(ExpenseItem))
                    {
                        await App.AlertSvc.ShowAlertAsync("NOTICE", "Deleted");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                        await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to delete Expense");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("Exception", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                dataService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
