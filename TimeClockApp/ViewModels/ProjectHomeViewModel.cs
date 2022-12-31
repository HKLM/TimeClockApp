using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ProjectHomeViewModel : TimeStampViewModel
    {
        protected ProjectDetailService cardService;

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject = new();
        partial void OnSelectedProjectChanged(Project value)
        {
            LoadDetails();
        }
        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = new();
        [ObservableProperty]
        private Phase? selectedPhase = new();
        partial void OnSelectedPhaseChanged(Phase? value)
        {
            LoadDetails();
        }

        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion
        [ObservableProperty]
        private DateOnly filterDateStart;
        [ObservableProperty]
        private DateOnly filterDateEnd;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NoFilterDate))]
        private bool useFilterDate;
        public bool NoFilterDate => !UseFilterDate;

        [ObservableProperty]
        private ObservableCollection<Expense> expenseList = new();

        [ObservableProperty]
        private bool showExpenseList;

        [ObservableProperty]
        private double totalHours = 0;

        [ObservableProperty]
        private double totalIncome = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalProject))]
        private double totalExpenses = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalProject))]
        [NotifyPropertyChangedFor(nameof(TotalEstimatedWComp))]
        private double total_Wages;
        public double TotalWages
        {
            get => Total_Wages;
            set => Total_Wages = (value * -1);
        }

        [ObservableProperty]
        private double totalProject;
        [ObservableProperty]
        private double totalProfitLoss;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalProject))]
        private double totalEstimated_WComp = 0;
        public double TotalEstimatedWComp
        {
            get => TotalEstimated_WComp;
            set => TotalEstimated_WComp = CalcWorkerComp(value);
        }

        private double CalcWorkerComp(double wagespaid) => wagespaid != 0 ? (wagespaid / 100) * WorkCompRate : 0;

        private double WorkCompRate = 0;

        [ObservableProperty]
        private double employerTaxes = 0;
        private double EmployerTaxRate = 0;

        [ObservableProperty]
        private double overHeadAmount = 0;

        private int OverHeadRate = 0;

        private bool is_Refreshing;
        public bool IsRefreshing
        {
            get { return is_Refreshing; }
            set { is_Refreshing = value; }
        }

        public ProjectHomeViewModel()
        {
            cardService = new();
            Total_Wages = 0;
            TotalHours = 0;
            TotalIncome = 0;
            TotalProfitLoss = 0;
            TotalEstimatedWComp = 0;
            TotalExpenses = 0;
            EmployerTaxes = 0;
            OverHeadRate = 0;
            OverHeadAmount = 0;

            ShowExpenseList = false;
            UseFilterDate = false;

            PickerMinDate = cardService.GetAppFirstRunDate();
            FilterDateEnd = DateOnly.FromDateTime(DateTime.Now);
            FilterDateStart = FilterDateEnd.AddDays(-7);
        }

        public void OnAppearing()
        {
            (WorkCompRate, EmployerTaxRate, OverHeadRate) = cardService.GetTaxRates();
            RefreshProjectPhases();
            LoadDetails();
        }

        private void RefreshProjectTotal()
        {
            TotalProject = TotalEstimatedWComp + TotalExpenses + TotalWages + EmployerTaxes;
            TotalProfitLoss = TotalProject + TotalIncome;
        }

        private void RefreshProjectPhases()
        {
            //Only get data from DB once, unless it has been notified that it has changed
            ProjectList ??= new();
            if (ProjectList.Any() == false || App.NoticeProjectHasChanged == true)
                ProjectList = cardService.GetProjectsList();

            PhaseList ??= new();
            if (PhaseList.Any() == false || App.NoticePhaseHasChanged == true)
                PhaseList = cardService.GetPhaseList();

            if (SelectedProject == null || SelectedProject.ProjectId == 0)
                SelectedProject = cardService.GetCurrentProjectEntity();
        }

        private void LoadDetails()
        {
            if (SelectedProject != null && SelectedPhase != null && SelectedPhase.PhaseId != 0)
            {
                if (UseFilterDate)
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = cardService.GetProjectDetails(SelectedProject.ProjectId, SelectedPhase.PhaseId, FilterDateStart, FilterDateEnd);
                else
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = cardService.GetProjectDetails(SelectedProject.ProjectId, SelectedPhase.PhaseId);
            }
            else if (SelectedProject != null && (SelectedPhase == null || SelectedPhase.PhaseId == 0))
            {
                if (UseFilterDate)
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = cardService.GetProjectDetails(SelectedProject.ProjectId, FilterDateStart, FilterDateEnd);
                else
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = cardService.GetProjectDetails(SelectedProject.ProjectId);
            }
            EmployerTaxes = TotalWages * EmployerTaxRate;
            TotalEstimatedWComp = TotalWages;
            RefreshProjectTotal();
        }

        private async Task<bool> LoadDetailsAsync()
        {
            if (SelectedProject != null && SelectedPhase != null && SelectedPhase.PhaseId != 0)
            {
                if (UseFilterDate)
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = await cardService.GetProjectDetailsAsync(SelectedProject.ProjectId, SelectedPhase.PhaseId, FilterDateStart, FilterDateEnd);
                else
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = await cardService.GetProjectDetailsAsync(SelectedProject.ProjectId, SelectedPhase.PhaseId);
            }
            else if (SelectedProject != null && (SelectedPhase == null || SelectedPhase.PhaseId == 0))
            {
                if (UseFilterDate)
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = await cardService.GetProjectDetailsAsync(SelectedProject.ProjectId, FilterDateStart, FilterDateEnd);
                else
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = await cardService.GetProjectDetailsAsync(SelectedProject.ProjectId);
            }
            EmployerTaxes = TotalWages * EmployerTaxRate;
            TotalEstimatedWComp = TotalWages;
            return true;
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;

            try
            {
                await Task.Run(async () =>
                {
                    if (await LoadDetailsAsync())
                        RefreshProjectTotal();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void ToggleFilterDate()
        {
            UseFilterDate = NoFilterDate;
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
