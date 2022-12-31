using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Helpers;
using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    [QueryProperty("IdTimeCard", "id")]
    public partial class EditTimeCardViewModel : TimeStampViewModel
    {
        protected EditTimeCardService cardService;
        public string IdTimeCard
        {
            set
            {
                if (value != null)
                {
                    if (int.TryParse(Uri.UnescapeDataString(value), out int i))
                    {
                        if (i > 0)
                            TimeCardID = i;
                    }
                }
            }
        }
        [ObservableProperty]
        private int timeCardID = 0;

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

        public EditTimeCardViewModel()
        {
            cardService = new();
            IsAdmin = IntToBool(cardService.GetConfigInt(9, 0));
        }

        public async Task OnAppearingAsync()
        {
            ProjectList = await cardService.GetProjectsListAsync();
            PhaseList = await cardService.GetPhaseListAsync();
            PickerMinDate = cardService.GetAppFirstRunDate();
            await RefreshCardAsync();
        }

        [RelayCommand]
        private async Task LoadTimeCardAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                if (TimeCardID > 0)
                {
                    if (TimeCardEditing == null || TimeCardEditing.TimeCardId == 0)
                    {
                        //should be null only at 1st loading
                        TimeCardEditing = await cardService.GetTimeCardByIDAsync(TimeCardID);
                        if (TimeCardEditing == null || TimeCardEditing.TimeCardId == 0) return;
                    }
                    if (TimeCardEditing != null && TimeCardEditing.TimeCardId != 0)
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
                        SelectedProject = await cardService.GetProjectAsync(TimeCardEditing.ProjectId);
                        SelectedPhase = await cardService.GetPhaseAsync(TimeCardEditing.PhaseId);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveTimeCardAsync()
        {
            if (IsBusy)
                return;

            if (TimeCard_EndTime < TimeCard_StartTime && (TimeCard_EndTime != new TimeOnly()))
            {
                await App.AlertSvc.ShowAlertAsync("ERROR", "StartTime must be before EndTime.");
                return;
            }
            IsBusy = true;

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

                if (cardService.UpdateTimeCard(TimeCardEditing))
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "TimeCard saved");
                else
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to save TimeCard");

                await RefreshCardAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("Exception", ex.Message + "\n" + ex.InnerException);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshCardAsync()
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

                    SelectedProject = await cardService.GetProjectAsync(TimeCardEditing.ProjectId);
                    SelectedPhase = await cardService.GetPhaseAsync(TimeCardEditing.PhaseId);
                }
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
