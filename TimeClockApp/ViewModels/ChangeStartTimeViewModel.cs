using TimeClockApp.Shared.Helpers;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class ChangeStartTimeViewModel(EditTimeCardService service) : BaseViewModel, IQueryAttributable
    {
        protected readonly EditTimeCardService cardService = service;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id") && Int32.TryParse(query["id"].ToString(), out int t))
            { TimeCardID = t; }
        }

        [ObservableProperty]
        public partial int TimeCardID { get; set; } = 0;

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
            HasError = false;
            PickerMinDate = cardService.GetAppFirstRunDate();

            try
            {
                List<Project> pro = await cardService.GetProjectsListAsync();
                ProjectList = pro.ToObservableCollection();
                List<Phase> ph = await cardService.GetPhaseListAsync();
                PhaseList = ph.ToObservableCollection();

                if (TimeCardID != 0)
                {
                    TimeCardEditing = await cardService.GetTimeCardByIDAsync(TimeCardID);
                    if (TimeCardEditing != null)
                    {
                        StartTime = TimeCardEditing.TimeCard_StartTime;
                        TimeCard_Date = TimeCardEditing.TimeCard_Date;
                        TimeCard_EmployeeName = TimeCardEditing.TimeCard_EmployeeName;
                        SelectedProject = TimeCardEditing.Project;
                        SelectedPhase = TimeCardEditing.Phase;
                    }
                }

            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "ChangeStartTimeViewModel.OnAppearing");
            }
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

                bool success = await cardService.UpdateTimeCardAsync(TimeCardEditing);
                string message = success ? "TimeCard saved" : "Failed to save TimeCard";
                await App.AlertSvc!.ShowAlertAsync("NOTICE", message).ConfigureAwait(false);

                if (success)
                    await RefreshCard();
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "ChangeStartTimeViewModel.SaveTimeCard");
            }
        }

        private async Task RefreshCard()
        {
            if (TimeCardID > 0)
            {
                TimeCardEditing = await cardService.GetTimeCardByIDAsync(TimeCardID);
                if (TimeCardEditing != null)
                {
                    StartTime = TimeCardEditing.TimeCard_StartTime;
                    TimeCard_Date = TimeCardEditing.TimeCard_Date;
                    TimeCard_EmployeeName = TimeCardEditing.TimeCard_EmployeeName;
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
