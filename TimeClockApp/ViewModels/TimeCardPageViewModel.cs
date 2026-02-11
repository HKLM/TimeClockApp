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
            if (value != null && SelectedProject != null && value.ProjectId != SelectedProject.ProjectId)
                Task.Run(() => cardService.SaveCurrentProjectAsync(value.ProjectId).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "TimeCardPageViewModel.OnSelectedProjectChanging");
                    }
                }));
        }

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();

        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase? value)
        {
            if (value != null && SelectedPhase != null && value.PhaseId != SelectedPhase.PhaseId)
                Task.Run(() => cardService.SaveCurrentPhaseAsync(value.PhaseId).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "TimeCardPageViewModel.OnSelectedPhaseChanging");
                    }
                }));
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            HasError = false;
            try
            {
                List<Project> proList = await cardService.GetProjectsListAsync();
                ProjectList = proList.ToObservableCollection();
                List<Phase> phaseList = await cardService.GetPhaseListAsync();
                PhaseList = phaseList.ToObservableCollection();

                SelectedProject = await cardService.GetCurrentProjectEntityAsync();
                SelectedPhase = await cardService.GetCurrentPhaseEntityAsync();

                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                TimeCards = t.ToObservableCollection();

            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "TimeCardPageViewModel.OnAppearing");
            }
        }

        [RelayCommand]
        private async Task ClockIn(TimeCard? card)
        {
            if (card != null && SelectedProject != null && SelectedPhase != null)
            {
                if (await cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle))
                {
                    List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                    TimeCards = t.ToObservableCollection();
                }
            }
        }

        [RelayCommand]
        private async Task ClockOutAsync(TimeCard? card)
        {
            if (card != null && await cardService.EmployeeClockOutAsync(card.TimeCardId))
            {
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                TimeCards = t.ToObservableCollection();
            }
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

            await Task.WhenAll(clockInTasks);

            List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
            TimeCards = t.ToObservableCollection();
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

            await Task.WhenAll(clockOutTasks);

            List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
            TimeCards = t.ToObservableCollection();
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
