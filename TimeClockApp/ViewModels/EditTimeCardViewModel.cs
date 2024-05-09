using TimeClockApp.Shared.Helpers;

namespace TimeClockApp.ViewModels
{
    [QueryProperty("IdTimeCard", "id")]
    [QueryProperty("ErrorCode", "e")]
    public partial class EditTimeCardViewModel(EditTimeCardService service) : TimeStampViewModel, IQueryAttributable
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
        private int timeCardEmployeeId;

        [ObservableProperty]
        private DateOnly timeCard_Date;
        partial void OnTimeCard_DateChanging(global::System.DateOnly oldValue, global::System.DateOnly newValue)
        {
            if (oldValue != new DateOnly())
                Last_TimeCard_Date = oldValue;
            else if (newValue != new DateOnly())
                Last_TimeCard_Date = newValue;
        }

        public DateOnly Last_TimeCard_Date { get; set; }

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

        [ObservableProperty]
        private ShiftStatus timeCard_Status = ShiftStatus.NA;
        public IReadOnlyList<string> AllShiftStatus { get; } = Enum.GetNames(typeof(ShiftStatus));
        [ObservableProperty]
        private int errorCode = 0!;
        partial void OnErrorCodeChanged(int value)
        {
            if (value == 1)
                Title = "ERROR IN CLOCKOUT TIME";
            else if (value == 2) 
                Title = "ERROR IN CLOCKIN TIME";
            else
                Title = "Edit TimeCard";
        }
#nullable enable

        [ObservableProperty]
        private string? title = "Edit TimeCard";
#nullable restore

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("e"))
            {
                if (Int32.TryParse(query["e"].ToString(), out int i))
                { ErrorCode = i; }
            }
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { TimeCardID = i; }
            }
        }

        public void OnAppearing()
        {
            PickerMinDate = cardService.GetAppFirstRunDate();
            if (ErrorCode > 0)
                IsAdmin = true;
            else
                IsAdmin = IntToBool(cardService.GetConfigInt(9, 0));
            RefreshProjectPhases();
            RefreshCard();
        }

        [RelayCommand]
        private async Task SaveTimeCardAsync()
        {
            if (TimeCard_EndTime < TimeCard_StartTime)
            {
                await App.AlertSvc.ShowAlertAsync("ERROR", "StartTime must be before EndTime.");
                return;
            }
            if (ErrorCode > 0 && TimeCard_Status == ShiftStatus.ClockedIn)
            {
                TimeCard_Status = ShiftStatus.ClockedOut;
            }

            try
            {
                if (TimeCardEditing == null || TimeCardID == 0 || TimeCardEditing.TimeCardId == 0)
                    return;

                TimeCardEditing.TimeCardId = TimeCardID;
                TimeCardEditing.TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(TimeCard_StartTime.Hour, TimeCard_StartTime.Minute));
                TimeCardEditing.TimeCard_Date = TimeCard_Date;
                TimeCardEditing.TimeCard_Status = TimeCard_Status;
                TimeCardEditing.TimeCard_bReadOnly = TimeCard_bReadOnly;
                TimeCardEditing.ProjectId = SelectedProject.ProjectId;
                TimeCardEditing.ProjectName = SelectedProject.Name;
                TimeCardEditing.PhaseId = SelectedPhase.PhaseId;
                TimeCardEditing.PhaseTitle = SelectedPhase.PhaseTitle;
                TimeCardEditing.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(TimeCard_EndTime.Hour, TimeCard_EndTime.Minute));

                if (cardService.UpdateTimeCard(TimeCardEditing, IsAdmin))
                {
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "TimeCard saved");
                }
                else
                {
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to save TimeCard");
                }

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
    }
}
