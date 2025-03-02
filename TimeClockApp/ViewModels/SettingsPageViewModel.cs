#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class SettingsPageViewModel : BaseViewModel
    {
        private readonly SettingsService configService;
        [ObservableProperty]
        public partial ObservableCollection<Config> SettingsList { get; set; } = new();

        [ObservableProperty]
        public partial string HelpInfo { get; set; } = "System configuration settings. Improper changes may crash the program.";

        public SettingsPageViewModel()
        {
            configService = new();
        }

        public void OnAppearing()
        {
            RefreshSettings();
        }

        [RelayCommand]
        private void SaveSetting(Config? item)
        {
            if (item == null) return;

            if (configService.SaveConfig(item))
                RefreshSettings();
        }

        [RelayCommand]
        private void DisplayHint(Config? item)
        {
            if (item == null) return;

            HelpInfo = string.IsNullOrEmpty(item.Hint) ? string.Empty : item.Hint;
            OnToggleHelpInfoBox();
        }

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
