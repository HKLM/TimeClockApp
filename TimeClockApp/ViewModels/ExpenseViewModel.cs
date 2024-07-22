using System.Xml.Linq;
using CommunityToolkit.Maui.Core.Extensions;

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
        private ObservableCollection<ExpenseType> expenseTypeList = [];
        [ObservableProperty]
        private ExpenseType selectedExpenseType = new();

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

        [ObservableProperty]
        private bool showAll = true;

        [ObservableProperty]
        private string memo = string.Empty;

        public ExpenseViewModel(ExpenseService service)
        {
            dataService = service;
            ShowArchived = false;
            PickerMinDate = dataService.GetAppFirstRunDate();
            pickerMaxDate = DateTime.Now;
        }

        [RelayCommand]
        private async Task InitAsync()
        {
            Loading = true;
            HasError = false;

            ExpenseList ??= new();
            ExpenseDate = DateOnly.FromDateTime(DateTime.Now);
            ShowAll = true;
            try
            {
                await GetExpenseListAsync();

                RefreshProjectPhases();
            }
            catch (Exception e)
            {
                HasError = true;
                System.Diagnostics.Trace.WriteLine(e.Message + "\n  -- " + e.Source + "\n  -- " + e.InnerException);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task OnAppearing()
        {
            Refresh();

            Loading = true;
            HasError = false;

            try
            {
                await GetExpenseListAsync();
            }
            finally
            {
                Loading = false;
            }
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

            ExpenseTypeList ??= [];

            List<ExpenseType> x = dataService.GetExpenseTypeList();
            ExpenseTypeList = x.ToObservableCollection();

            //default to materials
            //TODO make this user configurable
            SelectedExpenseType = dataService.GetExpenseType(5);
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
                //TODO remove this
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            IsRefreshingList = false;
        }

        [RelayCommand]
        private async Task GetExpenseListAsync()
        {
            await Task.Run(async () => await ExpenseListAsync(SelectedProject.ProjectId, ShowRecent, ShowAll));
        }
        private async Task ExpenseListAsync(int? projectId = null, bool useRecent = true, bool useShowAll = false)
        {
            IsRefreshingList = true;
            try
            {
                if (useShowAll)
                {
                    var L = await dataService.GetAllExpensesListAsync(20);
                    ExpenseList = L.ToObservableCollection();
                }
                else if (SelectedProject?.ProjectId > 0)
                {
                   var L  = await dataService.GetExpenseListAsync(projectId, useRecent, useShowAll);
                    ExpenseList = L.ToObservableCollection();
                }
            }
            finally
            {
                IsRefreshingList = false;
            }

        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            if (SelectedProject == null || SelectedProject.ProjectId < 1 || SelectedPhase == null || SelectedExpenseType == null)
                return;

            if (Amount == 0)
            {
                await App.AlertSvc.ShowAlertAsync("ERROR", "Amount can not be 0");
                return;
            }

            try
            {
                string expenseNewMemo = Memo.Trim();
                if (dataService.AddNewExpense(SelectedProject.ProjectId, SelectedPhase.PhaseId, Amount, expenseNewMemo, SelectedProject.Name, SelectedPhase.PhaseTitle, SelectedExpenseType.ExpenseTypeId, SelectedExpenseType.CategoryName))
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
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
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
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
                IsRefreshingList = false;
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
