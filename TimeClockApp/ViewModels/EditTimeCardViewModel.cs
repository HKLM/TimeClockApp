using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class EditTimeCardViewModel(EditTimeCardService service) : BaseViewModel, IQueryAttributable
    {
        protected readonly EditTimeCardService cardService = service;

        [ObservableProperty]
        public partial int TimeCardID { get; set; } = 0;

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
        public DateTime PickerMaxDate { get; } = DateTime.Now;
#endregion

        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = new();
        [ObservableProperty]
        public partial Project? SelectedProject { get; set; } = null;
        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = new();
        [ObservableProperty]
        public partial Phase? SelectedPhase { get; set; } = null;

        /// <summary>
        /// Page title bindable property
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageTitle))]
        public partial string? Title { get; set; } = string.Empty;
        /// <summary>
        /// Displays the page Title value
        /// </summary>
        public string? PageTitle => Title;

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
            if (query.ContainsKey("e") && int.TryParse(query["e"].ToString(), out int i))
            { ErrorCode = i; }
            if (query.ContainsKey("id") && int.TryParse(query["id"].ToString(), out int t))
            { TimeCardID = t; }
        }

        public async Task OnAppearing()
        {
            HasError = false;
            SetPageTitle(ErrorCode);
            PickerMinDate = cardService.GetAppFirstRunDate();
            IsAdmin = ErrorCode > 0 || IntToBool(cardService.GetConfigInt(9, 0));

            try
            {
                List<Project> pro = await cardService.GetProjectsListAsync();
                ProjectList = pro.ToObservableCollection();
                List<Phase> ph = await cardService.GetPhaseListAsync();
                PhaseList = ph.ToObservableCollection();

                if (TimeCardID != 0)
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
                        SelectedProject = TimeCardEditing.Project;
                        SelectedPhase = TimeCardEditing.Phase;
                    }
                }
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "EditTimeCardViewModel.OnAppearing");
            }
        }

        public void OnDisappearing()
        {
            //reset error code
            ErrorCode = 0;
        }

        [RelayCommand]
        private async Task SaveTimeCardAsync()
        {
            // Validate time range
            if (TimeCard_EndTime < TimeCard_StartTime)
            {
                await App.AlertSvc!.ShowAlertAsync("ERROR", "StartTime must be before EndTime.").ConfigureAwait(false);
                return;
            }

            // Validate required data
            if (TimeCardEditing == null || TimeCardID == 0 || TimeCardEditing.TimeCardId == 0 || 
                SelectedProject == null || SelectedPhase == null)
                return;

            // Check read-only status
            if (TimeCard_bReadOnly && IsAdmin)
            {
                bool confirm = await App.AlertSvc!.ShowConfirmationAsync("CONFIRM", "This TimeCard is currently read-only. Do you want to edit this TimeCard?");
                if (!confirm)
                    return;
            }
            if (await cardService.IsTimeCardReadOnlyAsync(TimeCardID, IsAdmin))
            {
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "This TimeCard is read-only and cannot be edited.").ConfigureAwait(false);
                return;
            }

            // Auto-fix status if needed
            if (ErrorCode > 0 && TimeCard_Status == ShiftStatus.ClockedIn)
                TimeCard_Status = ShiftStatus.ClockedOut;

            try
            {
                UpdateTimeCardEntity();

                bool success = await cardService.UpdateTimeCardAsync(TimeCardEditing, IsAdmin);
                string message = success ? "TimeCard saved" : "Failed to save TimeCard";
                await App.AlertSvc!.ShowAlertAsync("NOTICE", message).ConfigureAwait(false);

                if (success)
                    await RefreshCard();
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "EditTimeCardViewModel.SaveTimeCardAsync");
            }
        }

        private void UpdateTimeCardEntity()
        {
            TimeCardEditing!.TimeCardId = TimeCardID;
            TimeCardEditing.TimeCard_StartTime = TimeCard_StartTime;
            TimeCardEditing.TimeCard_Date = TimeCard_Date;
            TimeCardEditing.TimeCard_Status = TimeCard_Status;
            TimeCardEditing.TimeCard_bReadOnly = TimeCard_bReadOnly;
            TimeCardEditing.ProjectId = SelectedProject!.ProjectId;
            TimeCardEditing.ProjectName = SelectedProject.Name;
            TimeCardEditing.PhaseId = SelectedPhase!.PhaseId;
            TimeCardEditing.PhaseTitle = SelectedPhase.PhaseTitle;
            TimeCardEditing.TimeCard_EndTime = TimeCard_EndTime;
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
                    SelectedProject = TimeCardEditing.Project;
                    SelectedPhase = TimeCardEditing.Phase;
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
