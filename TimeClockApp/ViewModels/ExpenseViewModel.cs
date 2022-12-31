using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class ExpenseViewModel : TimeStampViewModel
    {
        protected ExpenseService dataService;

        [ObservableProperty]
        private Expense expenseItem = new();

        [ObservableProperty]
        private ObservableCollection<Expense> expenseList = new();

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject = new();
        partial void OnSelectedProjectChanged(global::TimeClockApp.Models.Project value)
        {
            if (value != null && dataService != null && value.ProjectId != 0)
                ExpenseList = dataService.GetAllExpenses(value.ProjectId);
        }

        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = new();
        [ObservableProperty]
        private Phase selectedPhase;
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Models.Phase value)
        {
            if (value != null && SelectedPhase != null && SelectedPhase.PhaseId != value.PhaseId)
                dataService.SaveCurrentPhase(value.PhaseId);
        }

        [ObservableProperty]
        private ExpenseType catagory = ExpenseType.Materials;
        public IReadOnlyList<string> AllCatagory { get; } = Enum.GetNames(typeof(ExpenseType));

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
        private bool isRefreshingList;
        [ObservableProperty]
        private bool showArchived;
        private bool ShowRecent => !ShowArchived;

        public ExpenseViewModel()
        {
            dataService = new();
            ShowArchived = false;
            PickerMinDate = dataService.GetAppFirstRunDate();
            pickerMaxDate = DateTime.Now;
        }

        public void OnAppearing()
        {
            ExpenseDate = DateOnly.FromDateTime(DateTime.Now);
            Refresh();
        }

        private void Refresh()
        {
            RefreshProjectPhases();
            if (SelectedProject != null && SelectedProject.ProjectId > 0)
                ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
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

            if (SelectedProject == null || SelectedProject.ProjectId == 0)
                SelectedProject = dataService.GetCurrentProjectEntity();

            if (SelectedPhase == null || SelectedPhase.PhaseId == 0)
                SelectedPhase = dataService.GetCurrentPhaseEntity();
        }

        [RelayCommand]
        private async Task RefreshExpenseList()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            IsRefreshingList = true;
            try
            {
                if (SelectedProject != null && SelectedProject.ProjectId > 0)
                    ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                IsRefreshingList = false;
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
            IsRefreshingList = false;
        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            if (IsBusy)
                return;
            if (SelectedProject == null || SelectedProject.ProjectId < 1 || SelectedPhase == null)
                return;

            if (Amount == 0)
            {
                await App.AlertSvc.ShowAlertAsync("ERROR", "Amount can not be 0");
                return;
            }
            IsBusy = true;

            try
            {
                if (dataService.AddNewExpense(SelectedProject.ProjectId, SelectedPhase.PhaseId, Amount, string.Empty, SelectedProject.Name, SelectedPhase.PhaseTitle, Catagory))
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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ArchiveExpenseList()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            IsRefreshingList = true;
            try
            {
                if (ExpenseList != null && ExpenseList.Any())
                    if (dataService.ArchiveExpense(ExpenseList))
                        ExpenseList = dataService.GetAllExpenses(SelectedProject.ProjectId, ShowRecent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                IsRefreshingList = false;
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
            IsRefreshingList = false;
        }

        [RelayCommand]
        private void ToggleShowArchived(bool ToggledValue)
        {
            if (IsBusy)
                return;

            IsBusy = true;

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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
