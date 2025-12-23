#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class AppViewModel(AppPageService pageService) : ObservableObject
    {
        private readonly AppPageService _pageService = pageService;

        [ObservableProperty]
        public partial Microsoft.Maui.ApplicationModel.AppTheme OverrideAppTheme { get; set; } = AppTheme.Unspecified;

        public async Task<Microsoft.Maui.ApplicationModel.AppTheme> GetThemeTypeAsync()
        {
            if (OverrideAppTheme == AppTheme.Unspecified)
                OverrideAppTheme = await _pageService.GetAppThemeSettingAsync(); //.ConfigureAwait(false);

            return OverrideAppTheme;
        }

        public async Task GetCurrentProjectIdAsync()
        {
            if (App.CurrentProjectId == null || !App.CurrentProjectId.HasValue || App.CurrentProjectId.Value == 0)
                App.CurrentProjectId = await _pageService.GetConfigIntAsync(3, 1);
        }

        public async Task GetCurrentPhaseIdAsync()
        {
            if (App.CurrentPhaseId == null || !App.CurrentPhaseId.HasValue || App.CurrentPhaseId.Value == 0)
                App.CurrentPhaseId = await _pageService.GetConfigIntAsync(4, 1);
        }
    }
}
