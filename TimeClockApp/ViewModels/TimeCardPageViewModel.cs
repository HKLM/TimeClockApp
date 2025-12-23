#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TimeCardPageViewModel(TimeCardService service) : BaseViewModel
    {
        protected readonly TimeCardService cardService = service;

        [ObservableProperty]
        public partial ObservableCollection<TimeCard> TimeCards { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = new();

        [ObservableProperty]
        public partial Project? SelectedProject { get; set; } = null;
        partial void OnSelectedProjectChanging(global::TimeClockApp.Shared.Models.Project? value)
        {
            if (value?.ProjectId != App.CurrentProjectId)
                Task.Run(() => cardService.SaveCurrentProjectAsync(value!.ProjectId));
        }

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();

        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase? value)
        {
            if (value?.PhaseId != App.CurrentPhaseId)
                Task.Run(() => cardService.SaveCurrentPhaseAsync(value!.PhaseId));
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            Loading = true;
            HasError = false;

            try
            {
                Task projectsTask = ProjectList.Count == 0 || App.NoticeProjectHasChanged
                    ? cardService.GetProjectsListAsync().ContinueWith(t => ProjectList = t.Result.ToObservableCollection())
                    : Task.CompletedTask;

                Task phasesTask = PhaseList.Count == 0 || App.NoticePhaseHasChanged
                    ? cardService.GetPhaseListAsync().ContinueWith(t => PhaseList = t.Result.ToObservableCollection())
                    : Task.CompletedTask;

                Task selectedProjectTask = App.NoticeProjectHasChanged || SelectedProject?.ProjectId == 0 || SelectedProject is null
                    ? cardService.GetCurrentProjectEntityAsync().ContinueWith(t => SelectedProject = t.Result)
                    : Task.CompletedTask;

                Task selectedPhaseTask = SelectedPhase?.PhaseId == 0 || SelectedPhase is null || App.NoticePhaseHasChanged
                    ? cardService.GetCurrentPhaseEntityAsync().ContinueWith(t => SelectedPhase = t.Result)
                    : Task.CompletedTask;

                await Task.WhenAll(projectsTask, phasesTask, selectedProjectTask, selectedPhaseTask);

                List<TimeCard> timeCards = await cardService.GetLastTimeCardForAllEmployeesAsync();
                TimeCards = timeCards.ToObservableCollection();
            }
            catch (AggregateException ax)
            {
                HasError = true;
                string message = FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "TimeCardPageViewModel");
                await App.AlertSvc!.ShowAlertAsync("Exception", message);
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

        private async Task RefreshTimeCardsAsync()
        {
            List<TimeCard> timeCards = await cardService.GetLastTimeCardForAllEmployeesAsync();
            TimeCards = timeCards.ToObservableCollection();
        }

        [RelayCommand]
        private async Task ClockIn(TimeCard? card)
        {
            if (card is not null && SelectedProject is not null && SelectedPhase is not null)
            {
                if (await cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle))
                    await RefreshTimeCardsAsync();
            }
        }

        [RelayCommand]
        private async Task ClockOutAsync(TimeCard? card)
        {
            if (card is not null && await cardService.EmployeeClockOutAsync(card.TimeCardId))
                await RefreshTimeCardsAsync();
        }

        [RelayCommand]
        private async Task ClockAll_InAsync()
        {
            if (TimeCards is null || SelectedProject is null || SelectedPhase is null)
                return;

            List<Task<bool>> clockInTasks = TimeCards
                .Where(item => item.TimeCard_Status != ShiftStatus.ClockedIn)
                .Select(item => cardService.EmployeeClockInAsync(item, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle))
                .ToList();

            bool[] results = await Task.WhenAll(clockInTasks);
            if (results.Any(x => x))
                await RefreshTimeCardsAsync();
        }

        [RelayCommand]
        private async Task ClockAll_OutAsync()
        {
            if (TimeCards is null)
                return;

            List<Task<bool>> clockOutTasks = TimeCards
                .Where(item => item.TimeCard_Status == ShiftStatus.ClockedIn)
                .Select(item => cardService.EmployeeClockOutAsync(item.TimeCardId))
                .ToList();

            bool[] results = await Task.WhenAll(clockOutTasks);
            if (results.Any(x => x))
                await RefreshTimeCardsAsync();
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
