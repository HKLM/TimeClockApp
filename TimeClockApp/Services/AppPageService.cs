#nullable enable

namespace TimeClockApp.Services
{
    public class AppPageService : SQLiteDataStore
    {
        private const int AppThemeConfigId = 13;
        private const string AppThemeConfigName = "AppTheme";
        private const string AppThemeConfigHint = "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)";

        /// <summary>
        /// Override the system theme preference.
        /// </summary>
        public AppTheme GetAppThemeSetting()
        {
            Config? config = Context.Config.Find(AppThemeConfigId);
            config ??= CreateAppThemeConfig();
            return (AppTheme)(config.IntValue ?? 0);
        }

        private Config CreateAppThemeConfig()
        {
            Config? config = new()
            {
                ConfigId = AppThemeConfigId,
                Name = AppThemeConfigName,
                IntValue = 0,
                Hint = AppThemeConfigHint
            };
            Context.Add(config);
            _ = Context.SaveChanges();
            return config;
        }
    }
}
