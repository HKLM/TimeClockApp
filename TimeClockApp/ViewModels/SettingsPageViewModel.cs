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
                HelpInfo = item.Hint;
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
            if (SettingsList.Any())
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                configService.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            base.Dispose();
        }
    }
}
