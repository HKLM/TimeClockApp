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
            if (value != null)
            {
                if (!App.CurrentProjectId.HasValue || App.CurrentProjectId.HasValue && value.ProjectId != App.CurrentProjectId.Value)
                    Task.Run(async () => await cardService.SaveCurrentProjectAsync(value.ProjectId).ConfigureAwait(false));
            }
        }

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();

        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;
        partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase? value)
        {
            if (value != null)
            {
                if (!App.CurrentPhaseId.HasValue || App.CurrentPhaseId.HasValue && value.PhaseId != App.CurrentPhaseId.Value)
                    Task.Run(async () => await cardService.SaveCurrentPhaseAsync(value.PhaseId).ConfigureAwait(false));
            }
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            Loading = true;
            HasError = false;

            try
            {
                //Only get data from DB once, unless it has been notified that it has changed
                if (ProjectList.Count == 0 || App.NoticeProjectHasChanged)
                {
                    Task<List<Project>> tp = cardService.GetProjectsListAsync();
                    List<Project> p = await tp;
                    ProjectList = p.ToObservableCollection();
                }

                if (PhaseList.Count == 0 || App.NoticePhaseHasChanged)
                {
                    Task<List<Phase>> tph = cardService.GetPhaseListAsync();
                    List<Phase> ph = await tph;
                    PhaseList = ph.ToObservableCollection();
                }

                if (App.NoticeProjectHasChanged || SelectedProject == null || SelectedProject.ProjectId == 0)
                {
                    var tcp = cardService.GetCurrentProjectEntityAsync();
                    Project cp = await tcp;
                    SelectedProject = cp;
                }

                if (SelectedPhase == null || SelectedPhase.PhaseId == 0 || App.NoticePhaseHasChanged)
                {
                    var tcph = cardService.GetCurrentPhaseEntityAsync();
                    Phase cph = await tcph;
                    SelectedPhase = cph;
                }
                List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                TimeCards = t.ToObservableCollection();
            }
            catch (AggregateException ax)
            {
                HasError = true;
                string z = FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "TimeCardPageViewModel");
                await App.AlertSvc!.ShowAlertAsync("Exception", z);
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

        [RelayCommand]
        private async Task ClockIn(TimeCard? card)
        {
            if (card != null && SelectedProject != null && SelectedPhase != null)
            {
                Task<bool> clockIn = cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle);
                if (await clockIn)
                {
                    List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                    TimeCards = t.ToObservableCollection();
                }
            }
        }

        [RelayCommand]
        private async Task ClockOutAsync(TimeCard? card)
        {
            if (card != null)
            {
                Task<bool> clockOut = cardService.EmployeeClockOutAsync(card.TimeCardId);
                if (await clockOut)
                {
                    List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                    TimeCards = t.ToObservableCollection();
                }
            }
        }

        [RelayCommand]
        private async Task ClockAll_InAsync()
        {
            if (TimeCards != null && SelectedProject != null && SelectedPhase != null)
            {
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
                    TimeCards = t.ToObservableCollection();
                }
            }
        }

        [RelayCommand]
        private async Task ClockAll_OutAsync()
        {
            if (TimeCards != null)
            {
                int i = 0;
                foreach (TimeCard item in TimeCards)
                {
                    if (item.TimeCard_Status == ShiftStatus.ClockedIn)
                    {
                        if (await cardService.EmployeeClockOutAsync(item.TimeCardId))
                            i++;
                    }
                }

                if (i > 0)
                {
                    List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
                    TimeCards = t.ToObservableCollection();
                }
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
