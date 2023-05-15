#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ProjectHomeViewModel : TimeStampViewModel
    {
        protected readonly ProjectDetailService cardService;

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
        protected readonly DateTime pickerMaxDate = DateTime.Now;
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
        private double totalWages;
        private double SetTotalWages(double wage) => (TotalWages = wage * -1);

        [ObservableProperty]
        private double totalProject;
        [ObservableProperty]
        private double totalProfitLoss;

        [ObservableProperty]
        private double employerTaxes;
        private double EmployerTaxRate = 0;

        [ObservableProperty]
        private double overHeadAmount = 0;
        private int OverHeadRate = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalProject))]
        private double totalEstimatedWComp = 0;

        private double CalcWorkerComp(double wagespaid) => wagespaid != 0 ? (wagespaid / 100) * WorkCompRate : 0;
        private double WorkCompRate = 0;

        [ObservableProperty]
        private bool isRefreshing;

        public ProjectHomeViewModel(ProjectDetailService service)
        {
            cardService = service;
            TotalWages = 0;
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

        public Task OnAppearingAsync()
        {
            (WorkCompRate, EmployerTaxRate, OverHeadRate) = cardService.GetTaxRates();
            RefreshProjectPhases();
            Task ld = LoadDetailsAsync();
            return ld;
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
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = cardService.GetProjectDetails(SelectedProject.ProjectId, null, FilterDateStart, FilterDateEnd);
                else
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = cardService.GetProjectDetails(SelectedProject.ProjectId);
            }
            SetTotalWages(TotalWages);
            EmployerTaxes = TotalWages * EmployerTaxRate;
            TotalEstimatedWComp = CalcWorkerComp(TotalWages);
            RefreshProjectTotal();
        }

        private async Task LoadDetailsAsync()
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
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = await cardService.GetProjectDetailsAsync(SelectedProject.ProjectId, null, FilterDateStart, FilterDateEnd);
                else
                    (TotalExpenses, TotalIncome, TotalWages, TotalHours) = await cardService.GetProjectDetailsAsync(SelectedProject.ProjectId);
            }
            SetTotalWages(TotalWages);
            EmployerTaxes = TotalWages * EmployerTaxRate;
            TotalEstimatedWComp = CalcWorkerComp(TotalWages);
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            Task load = LoadDetailsAsync();
            await load;
            RefreshProjectTotal();
        }

        [RelayCommand]
        private void ToggleFilterDate()
        {
            UseFilterDate = NoFilterDate;
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
                cardService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
