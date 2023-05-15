namespace TimeClockApp.ViewModels
{
    public partial class TimeCardPageViewModel : TimeStampViewModel, IDisposable
    {
        protected readonly TimeCardService cardService;
        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCards = new();

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject;

        partial void OnSelectedProjectChanging(global::TimeClockApp.Models.Project value)
        {
            if (value != null)
            {
                if (SelectedProject != null && SelectedProject.ProjectId != value.ProjectId)
                    cardService.SaveCurrentProject(value.ProjectId);
            }
        }
        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = new();
        [ObservableProperty]
        private Phase selectedPhase;
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Models.Phase value)
        {
            if (value != null && SelectedPhase != null && SelectedPhase.PhaseId != value.PhaseId)
                cardService.SaveCurrentPhase(value.PhaseId);
        }

        public TimeCardPageViewModel(TimeCardService service)
        {
            cardService = service;
        }

        public Task OnAppearing()
        {
            RefreshProjectPhases();
            Task r = RefreshCards();
            return r;
        }

#nullable enable
        [RelayCommand]
        private async Task ClockIn(TimeCard? card)
        {
            if (card == null) return;
            Task<bool> clockIn = cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId);
            if (await clockIn)
            {
                Task r = RefreshCards();
                await r;
            }
        }

        [RelayCommand]
        private async Task ClockOutAsync(TimeCard? card)
        {
            if (card == null) return;
            Task<bool> clockOut = cardService.EmployeeClockOutAsync(card);
            if (await clockOut)
            {
                Task r = RefreshCards();
                await r;
            }
        }
#nullable restore

        [RelayCommand]
        private async Task RefreshCards()
        {
            if (TimeCards != null && TimeCards.Any())
                TimeCards.Clear();
            Task<ObservableCollection<TimeCard>> cards = cardService.GetLastTimeCardForAllEmployeesAsync();
            TimeCards = await cards;
            if (TimeCards == null || TimeCards.Count == 0)
                await App.AlertSvc.ShowAlertAsync("ERROR", "You must have at least 1 employed, active, employee.\n\nGo to Tools / HR Dept to add new employees. Or to activate a deactivate employee, press the icon of a person with a gear on the toolbar.");
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

            if (SelectedProject == null || SelectedProject.ProjectId == 0 || App.NoticeProjectHasChanged == true)
                SelectedProject = cardService.GetCurrentProjectEntity();

            if (SelectedPhase == null || SelectedPhase.PhaseId == 0 || App.NoticePhaseHasChanged == true)
                SelectedPhase = cardService.GetCurrentPhaseEntity();
        }

        [RelayCommand]
        private async Task ClockAll_InAsync()
        {
            if (TimeCards == null) return;
            int i = 0;
            foreach (TimeCard item in TimeCards)
            {
                if (item.TimeCard_Status != ShiftStatus.ClockedIn)
                    if (await cardService.EmployeeClockInAsync(item, SelectedProject.ProjectId, SelectedPhase.PhaseId))
                        i++;
            }
            if (i > 0)
                await RefreshCards();
        }

        [RelayCommand]
        private async Task ClockAll_OutAsync()
        {
            if (TimeCards == null) return;

            int i = 0;
            foreach (TimeCard item in TimeCards)
            {
                if (item.TimeCard_Status == ShiftStatus.ClockedIn)
                {
                    if (await cardService.EmployeeClockOutAsync(item))
                        i++;
                }
            }

            if (i > 0)
                await RefreshCards();
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
            //TimeCards = null;
            //SelectedPhase = null;
            //ProjectList = null;
            base.Dispose();
        }
    }
}
