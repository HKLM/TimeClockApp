using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using TimeClockApp.Models;
using TimeClockApp.Services;

namespace TimeClockApp.ViewModels
{
    public partial class SettingsPageViewModel : TimeStampViewModel
    {
        private SettingsService configService;
        [ObservableProperty]
        private ObservableCollection<Config> settingsList = new();

        [ObservableProperty]
        private string helpInfo;

        public SettingsPageViewModel()
        {
            configService = new();
        }
        public void OnAppearing()
        {
            RefreshSettings();
        }

#nullable enable
        [RelayCommand]
        private void SaveSetting(Config? item)
        {
            if (IsBusy)
                return;
            if (item == null) return;

            IsBusy = true;

            try
            {
                if (configService.SaveConfig(item))
                    RefreshSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                configService.ShowPopupError(ex.Message + "\n" + ex.InnerException, "ERROR");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void DisplayHint(Config? item)
        {
            if (IsBusy)
                return;
            if (item == null) return;

            IsBusy = true;

            try
            {
                HelpInfo = item.Hint;
                OnToggleHelpInfoBox();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                configService.ShowPopupError(ex.Message + "\n" + ex.InnerException, "ERROR");
            }
            finally
            {
                IsBusy = false;
            }
        }

#nullable disable

        [RelayCommand]
        private void RefreshSettings()
        {
            if (SettingsList.Any())
                SettingsList.Clear();
            SettingsList = configService.GetSettingsList();
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisibile = !HelpInfoBoxVisibile;
            if (!HelpInfoBoxVisibile)
                HelpInfo = string.Empty;
        }
    }
}
