using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class EditPhaseViewModel : TimeStampViewModel
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

            //if (SelectedPhase != null)
            //{
            //    PhaseId = SelectedPhase.PhaseId;
            //    PhaseTitle = SelectedPhase.PhaseTitle;
            //}
        }

        [RelayCommand]
        private void SaveNewPhase()
        {
            if (IsBusy)
                return;

            IsBusy = true;

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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DeletePhase()
        {
            if (IsBusy)
                return;

            IsBusy = true;

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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SavePhaseEdit()
        {
            if (IsBusy)
                return;

            IsBusy = true;

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
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
        }
    }
}
