using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class EditTimeCardViewModel(EditTimeCardService service) : BaseViewModel, IQueryAttributable
    {
        protected readonly EditTimeCardService cardService = service;

        [ObservableProperty]
        public partial int TimeCardID { get; set; } = 0;
        partial void OnTimeCardIDChanged(int value)
        {
            MainThread.BeginInvokeOnMainThread(async () => { await RefreshCard(); });
        }

        [ObservableProperty]
        public partial TimeCard? TimeCardEditing { get; set; } = null;

        [ObservableProperty]
        public partial int TimeCardEmployeeId { get; set; }

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
        [NotifyPropertyChangedFor(nameof(EndTime))]
        public partial TimeOnly TimeCard_EndTime { get; set; } = new(0);
        public TimeOnly EndTime
        {
            get => TimeCard_EndTime;
            set => TimeCard_EndTime = TimeHelper.RoundTimeOnly(value);
        }

        [ObservableProperty]
        public partial double TimeCard_WorkHours { get; set; } = 0;
        [ObservableProperty]
        public partial string TimeCard_EmployeeName { get; set; } = string.Empty;
        [ObservableProperty]
        public partial double TimeCard_EmployeePayRate { get; set; } = 0.0;
        [ObservableProperty]
        public partial bool TimeCard_bReadOnly { get; set; } = false;
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

        [ObservableProperty]
        public partial ShiftStatus TimeCard_Status { get; set; } = ShiftStatus.NA;
        public IReadOnlyList<string> AllShiftStatus { get; } = Enum.GetNames<ShiftStatus>();

        //If the TimeCard failed the clockOut validation (TimeCardDataStore.ValidateClockOutAsync()), then this is used
        [ObservableProperty]
        public partial int ErrorCode { get; set; } = 0!;
        partial void OnErrorCodeChanged(int value)
        {
            SetPageTitle(value);
        }

        private string SetPageTitle(int value) => value switch
        {
            1 => Title = "ERROR IN CLOCKOUT TIME",
            2 => Title = "ERROR IN CLOCKIN TIME",
            3 => Title = "ERROR WITH THE DATE",
            _ => Title = "Edit TimeCard"
        };

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("e") && Int32.TryParse(query["e"].ToString(), out int i))
            { ErrorCode = i; }
            if (query.ContainsKey("id") && Int32.TryParse(query["id"].ToString(), out int t))
            { TimeCardID = t; }
        }

        public async Task OnAppearing()
        {
            SetPageTitle(ErrorCode);
            PickerMinDate = cardService.GetAppFirstRunDate();
            if (ErrorCode > 0)
                IsAdmin = true;
            else
                IsAdmin = IntToBool(cardService.GetConfigInt(9, 0));

            RefreshProjectPhases();
            await RefreshCard();
        }

        public void OnDisappearing()
        {
            //reset error code
            ErrorCode = 0;
        }

        [RelayCommand]
        private async Task SaveTimeCardAsync()
        {
            if (TimeCard_EndTime < TimeCard_StartTime)
            {
                await App.AlertSvc!.ShowAlertAsync("ERROR", "StartTime must be before EndTime.");
                return;
            }
            if (ErrorCode > 0 && TimeCard_Status == ShiftStatus.ClockedIn)
            {
                TimeCard_Status = ShiftStatus.ClockedOut;
            }

            if (TimeCardEditing == null || TimeCardID == 0 || TimeCardEditing.TimeCardId == 0 || SelectedProject == null || SelectedPhase == null)
                return;

            try
            {
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
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "TimeCard saved");
                }
                else
                {
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Failed to save TimeCard");
                }

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
