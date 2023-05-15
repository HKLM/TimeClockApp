namespace TimeClockApp.ViewModels
{
    public partial class EditPhaseViewModel : TimeStampViewModel, IDisposable
    {
        protected EditPhaseService phaseService;
        [ObservableProperty]
        private int phaseId = 0;
        [ObservableProperty]
        private string phaseTitle;

        [ObservableProperty]
        private ObservableCollection<Phase> phaseList = new();

        [ObservableProperty]
        private Phase selectedPhase;
        partial void OnSelectedPhaseChanged(global::TimeClockApp.Models.Phase value)
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
                if (PhaseTitle != null && PhaseTitle != "")
                {
                    phaseService.AddNewPhase(PhaseTitle);
                    App.NoticePhaseHasChanged = true;
                    RefreshPhases();
                    App.AlertSvc.ShowAlert("NOTICE", PhaseTitle + " saved");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DeletePhase()
        {
            try
            {
                if (SelectedPhase != null)
                {
                    if (await App.AlertSvc.ShowConfirmationAsync("CONFIRMATION", "Are you sure you want to Delete this expense?"))
                    {
                        string pTitle = SelectedPhase.PhaseTitle;
                        await phaseService.DeletePhase(SelectedPhase);
                        App.NoticePhaseHasChanged = true;
                        RefreshPhases();
                        await App.AlertSvc.ShowAlertAsync("NOTICE", pTitle + " deleted");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                await App.AlertSvc.ShowAlertAsync("ERROR", ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void SavePhaseEdit()
        {
            try
            {
                if (PhaseTitle != null && PhaseTitle != "")
                {
                    phaseService.UpdatePhase(PhaseTitle, PhaseId);
                    App.NoticePhaseHasChanged = true;
                    RefreshPhases();
                    App.AlertSvc.ShowAlert("NOTICE", PhaseTitle + " saved");
                }
                else if (PhaseId == 0)
                    App.AlertSvc.ShowAlert("Notice", "You must select a Phase before it can be updated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                App.AlertSvc.ShowAlert("ERROR", ex.Message + "\n" + ex.InnerException);
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
                phaseService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
