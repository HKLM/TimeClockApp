using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class TimeCardPageViewModel : TimeStampViewModel
    {
        protected TimeCardService cardService;
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

        public TimeCardPageViewModel()
        {
            cardService = new();
        }

        public Task OnAppearing()
        {
            IsBusy = false;
            RefreshProjectPhases();
            return RefreshCards();
        }

#nullable enable
        [RelayCommand]
        private async Task ClockIn(TimeCard? card)
        {
            if (IsBusy || card == null) return;
            IsBusy = true;

            if (await cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId))
                await RefreshCards();
            IsBusy = false;
        }

        [RelayCommand]
        private async Task ClockOutAsync(TimeCard? card)
        {
            if (IsBusy || card == null) return;
            IsBusy = true;

            if (await cardService.EmployeeClockOutAsync(card))
                await RefreshCards();

            IsBusy = false;
        }
#nullable restore

        [RelayCommand]
        private async Task RefreshCards()
        {
            if (TimeCards == null) return;
            if (TimeCards.Any())
                TimeCards.Clear();
            TimeCards = await cardService.GetLastTimeCardForAllEmployeesAsync();
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
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
