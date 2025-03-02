using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ChangeStartTimeViewModel(EditTimeCardService service) : BaseViewModel, IQueryAttributable
    {
        protected readonly EditTimeCardService cardService = service;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { TimeCardID = i; }
            }
        }

        [ObservableProperty]
        public partial int TimeCardID { get; set; } = 0;
        partial void OnTimeCardIDChanged(int value)
        {
            MainThread.BeginInvokeOnMainThread(async () => { await RefreshCard(); });
        }

        [ObservableProperty]
        public partial TimeCard? TimeCardEditing { get; set; } = null;
        [ObservableProperty]
        public partial DateOnly TimeCard_Date { get; set; }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StartTime))]
        public partial TimeOnly TimeCard_StartTime { get; set; } = new(0);
        public TimeOnly StartTime
        {
            get => TimeCard_StartTime;
            set => TimeCard_StartTime = TimeHelper.RoundTimeOnly(value);
        }

        [ObservableProperty]
        public partial string TimeCard_EmployeeName { get; set; } = string.Empty;

#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion
        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = [];
        [ObservableProperty]
        public partial Project? SelectedProject { get; set; } = null;
        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = [];
        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;

        public async Task OnAppearing()
        {
            PickerMinDate = cardService.GetAppFirstRunDate();
            RefreshProjectPhases();
            await RefreshCard();
        }

        [RelayCommand]
        private async Task SaveTimeCard()
        {
            if (TimeCardEditing == null || TimeCardEditing.TimeCardId == 0 || SelectedProject == null || SelectedPhase == null)
                return;

            try
            {
                TimeCardEditing.TimeCard_StartTime = TimeCard_StartTime;
                TimeCardEditing.ProjectId = SelectedProject.ProjectId;
                TimeCardEditing.ProjectName = SelectedProject.Name;
                TimeCardEditing.PhaseId = SelectedPhase.PhaseId;
                TimeCardEditing.PhaseTitle = SelectedPhase.PhaseTitle;

                if (cardService.UpdateTimeCard(TimeCardEditing))
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "TimeCard saved");

                await RefreshCard();
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        private void RefreshProjectPhases()
        {
            //Only get data from DB once, unless it has been notified that it has changed
            ProjectList ??= [];

            if (ProjectList.Count == 0 || App.NoticeProjectHasChanged)
                ProjectList = cardService.GetProjectsList();

            PhaseList ??= [];
            if (PhaseList.Count == 0 || App.NoticePhaseHasChanged)
                PhaseList = cardService.GetPhaseList();

            if (TimeCardID == 0)
            {
                if (SelectedProject == null || SelectedProject.ProjectId == 0)
                    SelectedProject = cardService.GetCurrentProjectEntity();

                if (SelectedPhase == null || SelectedPhase.PhaseId == 0)
                    SelectedPhase = cardService.GetCurrentPhaseEntity();
            }
        }

        private async Task RefreshCard()
        {
            if (TimeCardID > 0)
            {
                TimeCardEditing = await cardService.GetTimeCardByIDAsync(TimeCardID);
                if (TimeCardEditing != null && TimeCardEditing.TimeCardId != 0)
                {
                    StartTime = TimeCardEditing.TimeCard_StartTime;
                    TimeCard_Date = TimeCardEditing.TimeCard_Date;
                    TimeCard_EmployeeName = TimeCardEditing.TimeCard_EmployeeName;

                    SelectedProject = cardService.GetProject(TimeCardEditing.ProjectId);
                    SelectedPhase = cardService.GetPhase(TimeCardEditing.PhaseId);
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
