using CommunityToolkit.Maui.Core.Extensions;

namespace TimeClockApp.ViewModels
{
    public partial class EditExpensePageViewModel : TimeStampViewModel, IQueryAttributable
    {
        protected readonly ExpenseService dataService;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { ExpenseId = i; }
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
        private ObservableCollection<Project> projectList = [];
        [ObservableProperty]
        private Project selectedProject = new();

        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = [];
        [ObservableProperty]
        private Phase selectedPhase = new();

        [ObservableProperty]
        private ObservableCollection<ExpenseType> expenseTypeList = [];
        [ObservableProperty]
        private ExpenseType selectedExpenseType = new();

        //[ObservableProperty]
        //private string expenseType_CategoryName;

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
            IsAdmin = IntToBool(dataService.GetConfigInt(9, 0));

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
                SelectedProject = dataService.GetProject(ExpenseItem.ProjectId);
                SelectedPhase = dataService.GetPhase(ExpenseItem.PhaseId);
                SelectedExpenseType = dataService.GetExpenseType(ExpenseItem.ExpenseTypeId);
            }
        }

        private void RefreshProjectPhases()
        {
            //Only get data from DB once, unless it has been notified that it has changed
            ProjectList ??= [];
            if (!ProjectList.Any() || App.NoticeProjectHasChanged)
                ProjectList = dataService.GetAllProjectsList(IsAdmin);

            PhaseList ??= [];
            if (!PhaseList.Any() || App.NoticePhaseHasChanged)
                PhaseList = dataService.GetPhaseList();

            ExpenseTypeList ??= [];
            List<ExpenseType> x = dataService.GetExpenseTypeList();
            ExpenseTypeList = x.ToObservableCollection();
        }

        [RelayCommand]
        private async Task EditExpenseAsync()
        {
            if (ExpenseItem == null || ExpenseItem.ExpenseId == 0)
                return;
            if (string.IsNullOrEmpty(SelectedProject.Name))
            {
                await App.AlertSvc.ShowAlertAsync("NOTICE", "This record can not be edited. This maybe due to the Project has been archived and only a Admin can edit this record.");
                return;
            }

            try
            {
                string expenseNewMemo = Memo.Trim();
                ExpenseItem.Amount = Amount;
                ExpenseItem.Memo = expenseNewMemo;
                ExpenseItem.ExpenseDate = ExpenseDate;
                ExpenseItem.ProjectId = SelectedProject.ProjectId;
                ExpenseItem.PhaseId = SelectedPhase.PhaseId;
                ExpenseItem.ExpenseProject = SelectedProject.Name;
                ExpenseItem.ExpensePhase = SelectedPhase.PhaseTitle;
                ExpenseItem.ExpenseTypeId = SelectedExpenseType.ExpenseTypeId;
                ExpenseItem.ExpenseType_CategoryName = SelectedExpenseType.CategoryName;

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
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DelExpenseAsync()
        {
            if (ExpenseItem == null || ExpenseItem.ExpenseId == 0)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("User is attempting to delete a Expense record");
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
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
