#nullable enable

namespace TimeClockApp.Services
{
    public class AppPageService : SQLiteDataStore
    {
        /// <summary>
        /// Override the system theme preference.
        /// </summary>
        /// <returns></returns>
        public async Task<Microsoft.Maui.ApplicationModel.AppTheme> GetAppThemeSettingAsync()
        {
            Config? C = await Context.Config.FindAsync(13).ConfigureAwait(false);
            if (C == null)
            {
                //To update the database for prior versions
                C = new Config { ConfigId = 13, Name = "AppTheme", IntValue = 0, Hint = "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)" };
                Context.Add<Config>(C);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
            if (C != null && C.IntValue.HasValue)
                return (Microsoft.Maui.ApplicationModel.AppTheme)C.IntValue.Value;
            else
                return Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
        }

        public Microsoft.Maui.ApplicationModel.AppTheme GetAppThemeSetting()
        {
            Config? C = Context.Config.Find(13);
            if (C == null)
            {
                //To update the database for prior versions
                C = new Config { ConfigId = 13, Name = "AppTheme", IntValue = 0, Hint = "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)" };
                Context.Add<Config>(C);
                Context.SaveChanges();
            }
            if (C != null && C.IntValue.HasValue)
                return (Microsoft.Maui.ApplicationModel.AppTheme)C.IntValue.Value;
            else
                return Microsoft.Maui.ApplicationModel.AppTheme.Unspecified;
        }

    }
}
