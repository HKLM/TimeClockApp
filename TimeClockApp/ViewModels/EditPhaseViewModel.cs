namespace TimeClockApp.ViewModels
{
    public partial class EditPhaseViewModel : BaseViewModel
    {
        protected readonly EditPhaseService phaseService;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableSaveButton))]
        public partial int PhaseId { get; set; } = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAddDelButtons))]
        public partial string PhaseTitle { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = [];

        [ObservableProperty]
        public partial Phase SelectedPhase { get; set; }
        partial void OnSelectedPhaseChanged(global::TimeClockApp.Shared.Models.Phase value)
        {
            PhaseId = value.PhaseId;
            PhaseTitle = value.PhaseTitle;
        }

        public bool EnableAddDelButtons => !string.IsNullOrEmpty(PhaseTitle);
        public bool EnableSaveButton => PhaseId > 0;

        public EditPhaseViewModel()
        {
            phaseService = new();
        }

        public void OnAppearing()
        {
            RefreshPhases();
        }

        private void RefreshPhases()
        {
            if (PhaseList.Any())
                PhaseList.Clear();

            PhaseList = phaseService.GetEditPhaseList();
            PhaseTitle = string.Empty;
        }

        [RelayCommand]
        private void SaveNewPhase()
        {
            try
            {
                if (!string.IsNullOrEmpty(PhaseTitle))
                {
                    string phaseNewTitle = PhaseTitle.Trim();
                    phaseService.AddNewPhase(phaseNewTitle);
                    RefreshPhases();
                    App.AlertSvc!.ShowAlert("NOTICE", phaseNewTitle + " saved");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DeletePhase()
        {
            try
            {
                if (SelectedPhase != null)
                {
                    if (await App.AlertSvc!.ShowConfirmationAsync("CONFIRMATION", "Are you sure you want to Delete this expense?"))
                    {
                        string pTitle = SelectedPhase.PhaseTitle;
                        await phaseService.DeletePhase(SelectedPhase);
                        RefreshPhases();
                        await App.AlertSvc!.ShowAlertAsync("NOTICE", pTitle + " deleted").ConfigureAwait(false);
                    }
                }
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

        [RelayCommand]
        private void SavePhaseEdit()
        {
            try
            {
                if (!string.IsNullOrEmpty(PhaseTitle))
                {
                    string phaseNewTitle = PhaseTitle.Trim();
                    phaseService.UpdatePhase(phaseNewTitle, PhaseId);
                    RefreshPhases();
                    App.AlertSvc!.ShowAlert("NOTICE", phaseNewTitle + " saved");
                }
                else if (PhaseId == 0)
                    App.AlertSvc!.ShowAlert("Notice", "You must select a Phase before it can be updated");
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }
    }
}
