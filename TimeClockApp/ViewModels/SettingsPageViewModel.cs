namespace TimeClockApp.ViewModels
{
    public partial class SettingsPageViewModel : TimeStampViewModel
    {
        private readonly SettingsService configService;
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
            if (item == null) return;

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
        }

        [RelayCommand]
        private void DisplayHint(Config? item)
        {
            if (item == null) return;

            try
            {
                HelpInfo = item.Hint! ?? string.Empty;
                OnToggleHelpInfoBox();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                configService.ShowPopupError(ex.Message + "\n" + ex.InnerException, "ERROR");
            }
        }

#nullable disable

        [RelayCommand]
        private void RefreshSettings()
        {
            if (SettingsList.Count > 0)
                SettingsList.Clear();
            SettingsList = configService.GetSettingsList();
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
            if (!HelpInfoBoxVisible)
                HelpInfo = string.Empty;
        }
    }
}
