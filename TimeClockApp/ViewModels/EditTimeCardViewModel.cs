using TimeClockApp.Helpers;

namespace TimeClockApp.ViewModels
{
    [QueryProperty("IdTimeCard", "id")]
    public partial class EditTimeCardViewModel : TimeStampViewModel
    {
        protected readonly EditTimeCardService cardService;
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
        private int timeCardEmployeeId;

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
        [NotifyPropertyChangedFor(nameof(EndTime))]
        private TimeOnly timeCard_EndTime = new(0);
        public TimeOnly EndTime
        {
            get => TimeCard_EndTime;
            set => TimeCard_EndTime = TimeHelper.RoundTimeOnly(value);
        }

        [ObservableProperty]
        private double timeCard_WorkHours = 0;
        [ObservableProperty]
        private string timeCard_EmployeeName;
        [ObservableProperty]
        private double timeCard_EmployeePayRate = 0.0;
        [ObservableProperty]
        private bool timeCard_bReadOnly = false;
        #region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
        #endregion

        [ObservableProperty]
        private ObservableCollection<Project> projectList = new();
        [ObservableProperty]
        private Project selectedProject;
        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = new();
        [ObservableProperty]
        private Phase selectedPhase;

        [ObservableProperty]
        private ShiftStatus timeCard_Status = ShiftStatus.NA;
        public IReadOnlyList<string> AllShiftStatus { get; } = Enum.GetNames(typeof(ShiftStatus));

        public EditTimeCardViewModel(EditTimeCardService service)
        {
            cardService = service;
        }

        public void OnAppearing()
        {
            PickerMinDate = cardService.GetAppFirstRunDate();
            IsAdmin = IntToBool(cardService.GetConfigInt(9, 0));
            RefreshProjectPhases();
            RefreshCard();
        }

        [RelayCommand]
        private async Task SaveTimeCardAsync()
        {
            if (TimeCard_EndTime < TimeCard_StartTime && (TimeCard_EndTime != new TimeOnly()))
            {
                await App.AlertSvc.ShowAlertAsync("ERROR", "StartTime must be before EndTime.");
                return;
            }

            try
            {
                if (TimeCardEditing == null || TimeCardID == 0 || TimeCardEditing.TimeCardId == 0)
                    return;

                TimeCardEditing.TimeCardId = TimeCardID;
                TimeCardEditing.TimeCard_StartTime = TimeCard_StartTime;
                TimeCardEditing.TimeCard_Date = TimeCard_Date;
                TimeCardEditing.TimeCard_Status = TimeCard_Status;
                TimeCardEditing.TimeCard_bReadOnly = TimeCard_bReadOnly;
                TimeCardEditing.ProjectId = SelectedProject.ProjectId;
                TimeCardEditing.ProjectName = SelectedProject.Name;
                TimeCardEditing.PhaseId = SelectedPhase.PhaseId;
                TimeCardEditing.PhaseTitle = SelectedPhase.PhaseTitle;
                TimeCardEditing.TimeCard_EndTime = TimeCard_EndTime;

                if (cardService.UpdateTimeCard(TimeCardEditing, IsAdmin))
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "TimeCard saved");
                else
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to save TimeCard");

                RefreshCard();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("Exception", ex.Message + "\n" + ex.InnerException);
            }
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
                if (TimeCardEditing != null)
                {
                    TimeCard_StartTime = TimeCardEditing.TimeCard_StartTime;
                    TimeCard_Status = TimeCardEditing.TimeCard_Status;
                    TimeCard_WorkHours = TimeCardEditing.TimeCard_WorkHours;
                    TimeCard_bReadOnly = TimeCardEditing.TimeCard_bReadOnly;
                    TimeCard_Date = TimeCardEditing.TimeCard_Date;
                    TimeCard_EmployeeName = TimeCardEditing.TimeCard_EmployeeName;
                    TimeCard_EmployeePayRate = TimeCardEditing.TimeCard_EmployeePayRate;
                    TimeCardEmployeeId = TimeCardEditing.EmployeeId;
                    TimeCard_EndTime = TimeCardEditing.TimeCard_EndTime;

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
