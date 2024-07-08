using CommunityToolkit.Maui.Core.Extensions;
//#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class TimeCardPageViewModel(TimeCardService service) : TimeStampViewModel
    {
        protected readonly TimeCardService cardService = service;

        [ObservableProperty]
        private ObservableCollection<TimeCard> timeCards = new();

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new ();
        [ObservableProperty]
        private Project selectedProject;
        partial void OnSelectedProjectChanging(global::TimeClockApp.Shared.Models.Project value)
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
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase value)
        {
            if (value != null && SelectedPhase != null && SelectedPhase.PhaseId != value.PhaseId)
                cardService.SaveCurrentPhase(value.PhaseId);
        }

        [RelayCommand]
        private async Task InitAsync()
        {
            Loading = true;
            HasError = false;

            try
            {
                RefreshProjectPhases();
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                if (t?.Count > 0)
                    TimeCards = t.ToObservableCollection();
            }
            catch
            {
                HasError = true;
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task OnAppearing()
        {
            Loading = true;
            HasError = false;

            try
            {
                RefreshProjectPhases();
                var t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                if (t != null && t?.Count > 0)
                    TimeCards = t.ToObservableCollection();
            }
            catch
            {
                HasError = true;
                Debug.WriteLine("Error at TimeCardPageViewModel.OnAppearing()");
            }
            finally
            {
                Loading = false;
            }
        }

#nullable enable
        [RelayCommand]
        private async Task ClockIn(TimeCard? card)
        {
            if (card == null || SelectedProject == null || SelectedPhase == null) return;
            Task<bool> clockIn = cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle);
            if (await clockIn)
            {
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                if (t.Count > 0)
                    TimeCards = t.ToObservableCollection();
            }
        }

        [RelayCommand]
        private async Task ClockOutAsync(TimeCard? card)
        {
            if (card == null) return;
            Task<bool> clockOut = cardService.EmployeeClockOutAsync(card);
            if (await clockOut)
            {
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                if (t?.Count > 0)
                    TimeCards = t.ToObservableCollection();
            }
        }

        private void RefreshProjectPhases()
        {
            //Only get data from DB once, unless it has been notified that it has changed
            ProjectList ??= new ();
            if (ProjectList.Count == 0 || App.NoticeProjectHasChanged)
                ProjectList = cardService.GetProjectsList();

            PhaseList ??= new();
            if (PhaseList.Count == 0 || App.NoticePhaseHasChanged)
                PhaseList = cardService.GetPhaseList();

            if (SelectedProject == null || SelectedProject.ProjectId == 0 || App.NoticeProjectHasChanged)
                SelectedProject = cardService.GetCurrentProjectEntity();

            if (SelectedPhase == null || SelectedPhase.PhaseId == 0 || App.NoticePhaseHasChanged)
                SelectedPhase = cardService.GetCurrentPhaseEntity();
        }

        [RelayCommand]
        private async Task ClockAll_InAsync()
        {
            if (TimeCards == null || SelectedProject == null || SelectedPhase == null) return;
            int i = 0;
            foreach (TimeCard item in TimeCards)
            {
                if (item.TimeCard_Status != ShiftStatus.ClockedIn)
                {
                    if (await cardService.EmployeeClockInAsync(item, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle))
                        i++;
                }
            }
            if (i > 0)
            {
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                if (t?.Count > 0)
                    TimeCards = t.ToObservableCollection();
            }
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
            {
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                if (t?.Count > 0)
                    TimeCards = t.ToObservableCollection();
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
