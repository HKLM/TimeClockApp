#nullable enable

using TimeClockApp.Shared;

namespace TimeClockApp.Services
{
    public class AppPageService : SQLiteDataStore
    {
        /// <summary>
        /// Override the system theme preference.
        /// </summary>
        public AppTheme GetAppThemeSetting()
        {
            int aTheme;
            if (Preferences.ContainsKey("AppTheme", PrefSetting.PrefKey))
            {
                aTheme = Preferences.Get("AppTheme", 2, PrefSetting.PrefKey);
                return (AppTheme)aTheme;
            }
            Config? config = Context.Config.Find(13);
            config ??= CreateAppThemeConfig();            
            aTheme = config?.IntValue ?? 2;
            Preferences.Set("AppTheme", aTheme, PrefSetting.PrefKey);
            return (AppTheme)(aTheme);
        }

        private Config CreateAppThemeConfig()
        {
            Config? config = new()
            {
                ConfigId = 13,
                Name = "AppTheme",
                IntValue = 2,
                Hint = "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)"
            };
            Context.Add(config);
            _ = Context.SaveChanges();
            return config;
        }

        public bool GetNotificationsEnabledSetting()
        {
#if WINDOWS
			return false;
#else
            if (Preferences.ContainsKey("UseNotifications", PrefSetting.PrefKey))
            {
                return Preferences.Get("UseNotifications", false, PrefSetting.PrefKey);
            }
            else
            {
                Config? config = Context.Config.Find(14);
                if (config == null)
                {
                    config = new Config
                    {
                        ConfigId = 14,
                        Name = "NotificationsEnabled",
                        IntValue = 0,
                        Hint = "Enable or disable notifications (0=Disabled, 1=Enabled)"
                    };
                    Context.Add(config);
                    _ = Context.SaveChanges();
                }
                Preferences.Set("UseNotifications", config.IntValue == 1, PrefSetting.PrefKey);
                return config.IntValue == 1;
            }
#endif
        }

        public double GetNotificationHoursSetting()
        {
#if WINDOWS
			return 0;
#else
            double h = 0;
            if (Preferences.ContainsKey("NotificationHours", PrefSetting.PrefKey))
            {
                h = Preferences.Get("NotificationHours", 4.0, PrefSetting.PrefKey);
            }
            else
            {
                Config? config = Context.Config.Find(15);
                if (config == null)
                {
                    config = new Config
                    {
                        ConfigId = 15,
                        Name = "NotificationHours",
                        StringValue = "4.0",
                        Hint = "Amount of time in hours for notifications. In increments of a tenth of a hour. (1.5 = 1 hour & 30 min)"
                    };
                    Context.Add(config);
                    _ = Context.SaveChanges();
                }
                h = double.TryParse(config.StringValue ?? "0", out double result) ? result : 0;
                Preferences.Set("NotificationHours", h, PrefSetting.PrefKey);
            }
            return h;
#endif
        }

//        public async Task<double> GetNotificationHoursSettingAsync()
//        {
//#if WINDOWS
//			return 0;
//#else
//            if (Preferences.ContainsKey("NotificationHours", PrefSetting.PrefKey))
//            {
//                return Preferences.Get("NotificationHours", 4.0, PrefSetting.PrefKey);
//            }
//            else
//            {
//                Config? config = await Context.Config.FindAsync(15);
//                if (config == null)
//                {
//                    config = new Config
//                    {
//                        ConfigId = 15,
//                        Name = "NotificationHours",
//                        StringValue = "4.0",
//                        Hint = "Amount of time in hours for notifications. In increments of a tenth of a hour. (1.5 = 1 hour & 30 min)"
//                    };
//                    Context.Add(config);
//                    _ = await Context.SaveChangesAsync().ConfigureAwait(false);
//                }
//                double d = double.TryParse(config.StringValue ?? "0", out double result) ? result : 0;
//                Preferences.Set("NotificationHours", d, PrefSetting.PrefKey);
//                return d;
//            }
//#endif
//        }

//        public async Task SaveNotificationHoursSettingAsync(double hours)
//        {
//#if WINDOWS
//			return;
//#else
//            Preferences.Set("NotificationHours", hours, PrefSetting.PrefKey);
//            Config? config = await Context.Config.FindAsync(15);
//            if (config != null)
//            {
//                config.StringValue = hours.ToString("0.00");
//                Context.Update(config);
//                _ = await Context.SaveChangesAsync().ConfigureAwait(false);
//            }
//#endif
//        }
    }
}
