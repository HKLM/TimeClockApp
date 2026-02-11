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

        public async Task OnAppearing()
        {
            try
            {
                await RefreshSettings();
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "SettingsPageViewModel.OnAppearing");
            }
        }

        [RelayCommand]
        private async Task SaveSetting(Config? item)
        {
            if (item == null) return;

            if (await configService.SaveConfigAsync(item))
            {
                // Save the AppTheme setting
                if (item.ConfigId == 13 && item.IntValue.HasValue)
                {
                    Application.Current!.UserAppTheme = (AppTheme)item.IntValue!.Value;
                }
                await RefreshSettings();
            }
        }

        [RelayCommand]
        private void DisplayHint(Config? item)
        {
            if (item == null) return;

            HelpInfo = string.IsNullOrEmpty(item.Hint) ? string.Empty : item.Hint;
            OnToggleHelpInfoBox();
        }

        [RelayCommand]
        private async Task RefreshSettings()
        {
            try
            {
                if (SettingsList.Count > 0)
                    SettingsList.Clear();

                List<Config> S = await configService.GetSettingsListAsync();
                SettingsList = S.ToObservableCollection();
            }
            catch (Exception e)
            {
                HasError = true;
                Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "SettingsPageViewModel.RefreshSettings");
            }
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
