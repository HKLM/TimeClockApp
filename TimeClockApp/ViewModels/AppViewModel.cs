#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class AppViewModel(AppPageService pageService) : BaseViewModel
	{
		[ObservableProperty]
		public partial AppTheme OverrideAppTheme { get; set; } = AppTheme.Unspecified;

		public static double NotificationHours { get; set; } = 0;

		public AppTheme GetThemeType()
		{
			if (OverrideAppTheme == AppTheme.Unspecified)
				OverrideAppTheme = pageService.GetAppThemeSetting();

			return OverrideAppTheme;
		}

		private bool GetIsNotificationsEnabled()
		{
#if WINDOWS
			return false;
#else
			return pageService.GetNotificationsEnabledSetting();
#endif
		}

		public virtual void OnAppearing()
		{
			Application.Current!.UserAppTheme = GetThemeType();
			UseNotifications = GetIsNotificationsEnabled();
			if (UseNotifications)
				NotificationHours = pageService.GetNotificationHoursSetting();
		}	
	}
}
