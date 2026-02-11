#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class AppViewModel(AppPageService pageService) : ObservableObject
    {
        [ObservableProperty]
        public partial AppTheme OverrideAppTheme { get; set; } = AppTheme.Unspecified;

        public AppTheme GetThemeType()
        {
            if (OverrideAppTheme == AppTheme.Unspecified)
                OverrideAppTheme =  pageService.GetAppThemeSetting();

            return OverrideAppTheme;
        }
    }
}
