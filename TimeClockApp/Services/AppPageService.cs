#nullable enable

namespace TimeClockApp.Services
{
	public class AppPageService : SQLiteDataStore
	{
		private const string _appThemeConfigName = "AppTheme";
		private const string _appThemeConfigHint = "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)";
		private const string _notificationsEnabledConfigName = "NotificationsEnabled";
		private const string _notificationsEnabledConfigHint = "Enable or disable notifications (0=Disabled, 1=Enabled)";

		/// <summary>
		/// Override the system theme preference.
		/// </summary>
		public AppTheme GetAppThemeSetting()
		{
			Config? config = Context.Config.Find(13);
			config ??= CreateAppThemeConfig();
			return (AppTheme)(config.IntValue ?? 0);
		}

		private Config CreateAppThemeConfig()
		{
			Config? config = new()
			{
				ConfigId = 13,
				Name = _appThemeConfigName,
				IntValue = 0,
				Hint = _appThemeConfigHint
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
			Config? config = Context.Config.Find(14);
			if (config == null)
			{
				config = new Config
				{
					ConfigId = 14,
					Name = _notificationsEnabledConfigName,
					IntValue = 1,
					Hint = _notificationsEnabledConfigHint
				};
				Context.Add(config);
				_ = Context.SaveChanges();
			}
			return config.IntValue == 1;
#endif
		}

		public double GetNotificationHoursSetting()
		{
#if WINDOWS
			return 0;
#else
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
			return double.TryParse(config.StringValue ?? "0", out double result) ? result : 0;
#endif
		}

		public async Task<double> GetNotificationHoursSettingAsync()
		{
#if WINDOWS
			return 0;
#else
			Config? config = await Context.Config.FindAsync(15);
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
				_ = await Context.SaveChangesAsync().ConfigureAwait(false);
			}
			return double.TryParse(config.StringValue ?? "0", out double result) ? result : 0;
#endif
		}

		public async Task SaveNotificationHoursSettingAsync(double hours)
		{
#if WINDOWS
			return;
#else
			Config? config = await Context.Config.FindAsync(15);
			if (config != null)
			{
				config.StringValue = hours.ToString("0.00");
				Context.Update(config);
				_ = await Context.SaveChangesAsync().ConfigureAwait(false);
			}
#endif
		}
	}
}
