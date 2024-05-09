namespace TimeClockApp.ViewModels
{
    public partial class ExpenseViewModel : TimeStampViewModel
    {
        protected readonly ExpenseService dataService;

        [ObservableProperty]
        private Expense expenseItem = new();

        [ObservableProperty]
        private ObservableCollection<Expense> expenseList = [];

        [ObservableProperty]
        private ObservableCollection<Project> projectList = [];
        [ObservableProperty]
        private Project selectedProject = new();
        partial void OnSelectedProjectChanged(global::TimeClockApp.Shared.Models.Project value)
        {
            if (value != null && dataService != null && value.ProjectId != 0)
                ExpenseList = dataService.GetAllExpenses(value.ProjectId);
        }

        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = [];
        [ObservableProperty]
        private Phase selectedPhase;
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase value)
        {
            if (value != null && SelectedPhase != null && SelectedPhase.PhaseId != value.PhaseId)
                dataService.SaveCurrentPhase(value.PhaseId);
        }

        [ObservableProperty]
        private ExpenseType category = ExpenseType.Materials;
        public IReadOnlyList<string> AllCategory { get; } = Enum.GetNames(typeof(ExpenseType));

        [ObservableProperty]
        private DateOnly expenseDate;
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion

        [ObservableProperty]
        private Double amount;
        [ObservableProperty]
        private bool isRefreshingList;
        [ObservableProperty]
        private bool showArchived;
        private bool ShowRecent => !ShowArchived;

        public ExpenseViewModel(ExpenseService service)
        {
            dataService = service;
            ShowArchived = false;
            PickerMinDate = dataService.GetAppFirstRunDate();
            pickerMaxDate = DateTime.Now;
            Category = ExpenseType.Materials;
        }

        public void OnAppearing()
        {
            ExpenseDate = DateOnly.FromDateTime(DateTime.Now);
            Refresh();
        }

        private void Refresh()
        {
            RefreshProjectPhases();
            if (SelectedProject?.ProjectId > 0)
                ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
        }

        private void RefreshProjectPhases()
        {
            //Only get data from DB once, unless it has been notified that it has changed
            ProjectList ??= [];
            if (!ProjectList.Any() || App.NoticeProjectHasChanged)
                ProjectList = dataService.GetProjectsList();

            PhaseList ??= [];
            if (!PhaseList.Any() || App.NoticePhaseHasChanged)
                PhaseList = dataService.GetPhaseList();

            if (SelectedProject == null || SelectedProject.ProjectId == 0)
                SelectedProject = dataService.GetCurrentProjectEntity();

            if (SelectedPhase == null || SelectedPhase.PhaseId == 0)
                SelectedPhase = dataService.GetCurrentPhaseEntity();
        }

        [RelayCommand]
        private async Task RefreshExpenseList()
        {
            IsRefreshingList = true;
            try
            {
                if (SelectedProject?.ProjectId > 0)
                    ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                IsRefreshingList = false;
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            IsRefreshingList = false;
        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            if (SelectedProject == null || SelectedProject.ProjectId < 1 || SelectedPhase == null)
                return;

            if (Amount == 0)
            {
                await App.AlertSvc.ShowAlertAsync("ERROR", "Amount can not be 0");
                return;
            }

            try
            {
                if (dataService.AddNewExpense(SelectedProject.ProjectId, SelectedPhase.PhaseId, Amount, string.Empty, SelectedProject.Name, SelectedPhase.PhaseTitle, Category))
                {
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Saved");
                    Amount = 0;
                    Refresh();
                }
                else
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to save Expense");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void ArchiveExpenseList()
        {
            IsRefreshingList = true;
            try
            {
                if (ExpenseList?.Any() == true)
                    if (dataService.ArchiveExpense(ExpenseList))
                        ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                IsRefreshingList = false;
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            IsRefreshingList = false;
        }

        [RelayCommand]
        private void ToggleShowArchived(bool ToggledValue)
        {
            try
            {
                ShowArchived = ToggledValue;
                ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
