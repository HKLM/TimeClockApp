using TimeClockApp.Shared.Helpers;

namespace TimeClockApp.ViewModels
{
    [QueryProperty("IdTimeCard", "id")]
    public partial class ChangeStartTimeViewModel(EditTimeCardService service) : TimeStampViewModel
    {
        protected readonly EditTimeCardService cardService = service;
        public string IdTimeCard
        {
            set
            {
                if (value != null
                    && int.TryParse(Uri.UnescapeDataString(value), out int i))
                {
                    if (i > 0)
                        TimeCardID = i;
                }
            }
        }

        [ObservableProperty]
        private int timeCardID = 0;
        partial void OnTimeCardIDChanged(int value)
        {
            RefreshCard();
        }

        [ObservableProperty]
        private TimeCard timeCardEditing = new();
        [ObservableProperty]
        private DateOnly timeCard_Date;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StartTime))]
        private TimeOnly timeCard_StartTime = new(0);
        public TimeOnly StartTime
        {
            get => TimeCard_StartTime;
            set => TimeCard_StartTime = TimeHelper.RoundTimeOnly(value);
        }

        [ObservableProperty]
        private string timeCard_EmployeeName = "";
        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion
        [ObservableProperty]
        private ObservableCollection<Project> projectList = [];
        [ObservableProperty]
        private Project selectedProject;
        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = [];
        [ObservableProperty]
        private Phase selectedPhase;

        public void OnAppearing()
        {
            PickerMinDate = cardService.GetAppFirstRunDate();
            RefreshProjectPhases();
            RefreshCard();
        }

        [RelayCommand]
        private void SaveTimeCard()
        {
            if (TimeCardEditing != null && TimeCardEditing.TimeCardId != 0)
            {
                TimeCardEditing.TimeCard_StartTime = TimeCard_StartTime;
                TimeCardEditing.ProjectId = SelectedProject.ProjectId;
                TimeCardEditing.ProjectName = SelectedProject.Name;
                TimeCardEditing.PhaseId = SelectedPhase.PhaseId;
                TimeCardEditing.PhaseTitle = SelectedPhase.PhaseTitle;

                cardService.UpdateTimeCard(TimeCardEditing);
                App.AlertSvc.ShowAlert("NOTICE", "TimeCard saved");

                RefreshCard();
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

        private void RefreshCard()
        {
            if (TimeCardID > 0)
            {
                TimeCardEditing = cardService.GetTimeCardByID(TimeCardID);
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
