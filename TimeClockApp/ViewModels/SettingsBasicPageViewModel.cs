using TimeClockApp.Shared;
using static TimeClockApp.Shared.Enums;
#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class SettingsBasicPageViewModel(SettingsService configService, IProjectListService projectListService) : BaseViewModel
	{
		private readonly SettingsService configService = configService;
		private readonly IProjectListService projectListService = projectListService;
		[ObservableProperty]
		public partial int Config_DaysInPayPeriod { get; set; }
		[ObservableProperty]
		public partial DayOfWeek Config_PayDayOfWeek { get; set; }
		public IReadOnlyList<string> AllWeekDayNames { get; } = Enum.GetNames<DayOfWeek>();
		[ObservableProperty]
		public partial int ConfigIntPayDayOfWeek { get; set; }
		partial void OnConfigIntPayDayOfWeekChanged(int value)
		{
			Config_PayDayOfWeek = (DayOfWeek)value;
		}

		[ObservableProperty]
		public partial int Config_IsAdmin { get; set; }
		[ObservableProperty]
		public partial bool Config_UseNotifications { get; set; }
		[ObservableProperty]
		public partial double Config_NotificationHours { get; set; }

		[ObservableProperty]
		public partial int SelectedHours { get; set; } = 0;
		[ObservableProperty]
		public partial int SelectedMinutes { get; set; } = 0;
		[ObservableProperty]
		public partial ObservableCollection<int> PickerHours { get; set; } = Enumerable.Range(0, 24).ToObservableCollection();
		[ObservableProperty]
		public partial ObservableCollection<int> PickerMinutes { get; set; } = new();

		[ObservableProperty]
		public partial double Config_WorkCompRate { get; set; }
		[ObservableProperty]
		public partial double Config_ProfitRate { get; set; }
		[ObservableProperty]
		public partial double Config_OverHeadRate { get; set; }
		[ObservableProperty]
		public partial double Config_EstimateEmployerTaxes { get; set; }
		[ObservableProperty]
		public partial DateOnly Config_FirstRun { get; set; }
#region "DatePicker Min/Max Bindings"
		public DateTime PickerMinDate { get; set; }
		public DateTime PickerMaxDate { get; } = DateTime.Now;
#endregion

		[ObservableProperty]
		public partial ObservableCollection<Project> ProjectList { get; set; } = new();
		[ObservableProperty]
		public partial Project? CurrentProject { get; set; } = null;
		[ObservableProperty]
		public partial int Config_CurrentProject { get; set; } = 0;
		partial void OnConfig_CurrentProjectChanged(int value)
		{
			if (value > 0 && (CurrentProject == null || CurrentProject?.ProjectId != value))
			{
				CurrentProject = ProjectList.FirstOrDefault(p => p.ProjectId == value);
			}
		}

		[ObservableProperty]
		public partial ObservableCollection<Phase> PhaseList { get; set; } = new();
		[ObservableProperty]
		public partial Phase? CurrentPhase { get; set; } = null;

		[ObservableProperty]
		public partial int Config_CurrentPhase { get; set; } = 0;
		partial void OnConfig_CurrentPhaseChanged(int value)
		{
			if (value > 0 && (CurrentPhase == null || CurrentPhase?.PhaseId != value))
			{
				CurrentPhase = PhaseList.FirstOrDefault(p => p.PhaseId == value);
			}
		}

		[ObservableProperty]
		public partial AppThemeType Config_AppTheme { get; set; } = AppThemeType.System;
		public IReadOnlyList<string> AllAppThemes { get; } = Enum.GetNames<AppThemeType>();
		[ObservableProperty]
		public partial int ConfigIntAppTheme { get; set; } = 0;
		partial void OnConfigIntAppThemeChanged(int value)
		{
			Config_AppTheme = (AppThemeType)value;
		}

		[RelayCommand]
		public async Task OnAppearing()
		{
			Loading = true;
			HasError = false;

			try
			{
				PickerHours ??= Enumerable.Range(0, 24).ToObservableCollection();
				if (PickerHours.Count == 0)
				{
					PickerHours = Enumerable.Range(0, 24).ToObservableCollection();
				}
				PickerMinutes ??= [];
				if (PickerMinutes.Count == 0)
				{
					for (int i = 0; i < 60; i += 5)
					{
						PickerMinutes.Add(i);
					}
				}
				List<Project> pro = await projectListService.GetProjectsListAsync();
				ProjectList = pro.ToObservableCollection();
				List<Phase> ph = await projectListService.GetPhaseListAsync();
				PhaseList = ph.ToObservableCollection();
				PickerMinDate = configService.GetAppFirstRunDate();

				// Load settings from database if not already in preferences
				if (!Preferences.ContainsKey("CurrentProject", PrefSetting.PrefKey))
				{
					List<Config> S = await configService.GetSettingsListAsync();
					foreach (Config item in S)
					{
						if (item.ConfigId == 2)
						{
							if (!string.IsNullOrEmpty(item.StringValue))
							{
								Preferences.Set("FirstRun", item.StringValue, PrefSetting.PrefKey);
								if (DateOnly.TryParse(item.StringValue, out DateOnly runFirstDate))
								{
									Config_FirstRun = runFirstDate;
								}
							}
						}
						else if (item.ConfigId == 3)
						{
							Config_CurrentProject = item.IntValue ?? 1;
							Preferences.Set("CurrentProject", Config_CurrentProject, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 4)
						{
							Config_CurrentPhase = item.IntValue ?? 1;
							Preferences.Set("CurrentPhase", Config_CurrentPhase, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 5)
						{
							Config_WorkCompRate = item.StringValue != null ? double.Parse(item.StringValue) : 0.171118;
							Preferences.Set("WorkCompRate", Config_WorkCompRate, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 6)
						{
							Config_EstimateEmployerTaxes = item.StringValue != null ? double.Parse(item.StringValue) : 0.1019;
							Preferences.Set("EstimateEmployerTaxes", Config_EstimateEmployerTaxes, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 7)
						{
							Config_ProfitRate = item.StringValue != null ? double.Parse(item.StringValue) : 0.1;
							Preferences.Set("ProfitRate", Config_ProfitRate, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 8)
						{
							Config_OverHeadRate = item.StringValue != null ? double.Parse(item.StringValue) : 0.08;
							Preferences.Set("OverHeadRate", Config_OverHeadRate, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 9)
						{
							Config_IsAdmin = item.IntValue.GetValueOrDefault(0);
							Preferences.Set("IsAdmin", Config_IsAdmin, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 10)
						{
							Config_DaysInPayPeriod = item.IntValue ?? 7;
							Preferences.Set("DaysInPayPeriod", Config_DaysInPayPeriod, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 11)
						{
							ConfigIntPayDayOfWeek = item.IntValue ?? 5;
							Preferences.Set("PayDayOfWeek", ConfigIntPayDayOfWeek, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 13)
						{
							ConfigIntAppTheme = item.IntValue ?? 0;
							Preferences.Set("AppTheme", ConfigIntAppTheme, PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 14)
						{
							Config_UseNotifications = item.IntValue == 1;
							Preferences.Set("UseNotifications", item.IntValue.GetValueOrDefault(0), PrefSetting.PrefKey);
						}
						else if (item.ConfigId == 15)
						{
							ExtractAlarmFromString(item.StringValue!);
							Config_NotificationHours = item.StringValue != null ? double.Parse(item.StringValue) : 4.0;
							Preferences.Set("NotificationHours", Config_NotificationHours, PrefSetting.PrefKey);
						}
					}
				}
				else
				{
					Config_CurrentProject = Preferences.Get("CurrentProject", 1, PrefSetting.PrefKey);
					Config_CurrentPhase = Preferences.Get("CurrentPhase", 1, PrefSetting.PrefKey);
					Config_WorkCompRate = Preferences.Get("WorkCompRate", 0.171118, PrefSetting.PrefKey);
					Config_EstimateEmployerTaxes = Preferences.Get("EstimateEmployerTaxes", 0.1019, PrefSetting.PrefKey);
					Config_ProfitRate = Preferences.Get("ProfitRate", 0.1, PrefSetting.PrefKey);
					Config_OverHeadRate = Preferences.Get("OverHeadRate", 0.08, PrefSetting.PrefKey);

					Config_IsAdmin = Preferences.Get("IsAdmin", 0, PrefSetting.PrefKey);
					Config_DaysInPayPeriod = Preferences.Get("DaysInPayPeriod", 7, PrefSetting.PrefKey);
					ConfigIntPayDayOfWeek = Preferences.Get("PayDayOfWeek", 5, PrefSetting.PrefKey);
					ConfigIntAppTheme = Preferences.Get("AppTheme", 0, PrefSetting.PrefKey);
					int uNotif = Preferences.Get("UseNotifications", 0, PrefSetting.PrefKey);
					Config_UseNotifications = uNotif == 1;
					Config_NotificationHours = Preferences.Get("NotificationHours", 4.0, PrefSetting.PrefKey);
					ExtractAlarmFromString(Config_NotificationHours.ToString("0.00"));

					if (DateOnly.TryParse(Preferences.Get("FirstRun", PickerMinDate.ToShortDateString(), PrefSetting.PrefKey), out DateOnly rundate))
						Config_FirstRun = rundate;
				}
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "SettingsBasicPageViewModel.OnAppearing");
			}
			finally
			{
				Loading = false;
			}
		}

		[RelayCommand]
		private async Task SaveSettings()
		{
			HasError = false;

			// FirstRun
			if (Config_FirstRun > DateOnly.FromDateTime(DateTime.Now))
			{
				HasError = true;
				Log.WriteLine("The FirstRun setting cannot be set to a future date.", "SettingsBasicPageViewModel.SaveSetting");
				await App.AlertSvc!.ShowAlertAsync("WARNING", "The FirstRun setting cannot be set to a future date.").ConfigureAwait(false);
				return;
			}

			if (Loading)
				return;

			Loading = true;

			try
			{
				if (CurrentProject != null)
				{
					Preferences.Set("CurrentProject", CurrentProject!.ProjectId, PrefSetting.PrefKey);
				}
				if (CurrentPhase != null)
				{
					Preferences.Set("CurrentPhase", CurrentPhase!.PhaseId, PrefSetting.PrefKey);
				}
				Preferences.Set("WorkCompRate", Config_WorkCompRate, PrefSetting.PrefKey);
				Preferences.Set("EstimateEmployerTaxes", Config_EstimateEmployerTaxes, PrefSetting.PrefKey);
				Preferences.Set("ProfitRate", Config_ProfitRate, PrefSetting.PrefKey);
				Preferences.Set("OverHeadRate", Config_OverHeadRate, PrefSetting.PrefKey);
				if (Config_IsAdmin > 1)
					Config_IsAdmin = 1;

				Preferences.Set("IsAdmin", Config_IsAdmin, PrefSetting.PrefKey);
				IsAdmin = Config_IsAdmin == 1;

				Preferences.Set("DaysInPayPeriod", Config_DaysInPayPeriod, PrefSetting.PrefKey);
				Preferences.Set("PayDayOfWeek", (int)Config_PayDayOfWeek, PrefSetting.PrefKey);
				Preferences.Set("AppTheme", (int)Config_AppTheme, PrefSetting.PrefKey);
				Preferences.Set("UseNotifications", Config_UseNotifications ? 1 : 0, PrefSetting.PrefKey);

				string alarmString = $"{SelectedHours}.{SelectedMinutes:00}";
				if (double.TryParse(alarmString, out double alarmValue))
				{
					//await configService.SaveNotificationHoursSettingAsync(alarmValue);
					AppViewModel.NotificationHours = alarmValue;
				}

				Preferences.Set("NotificationHours", Config_NotificationHours, PrefSetting.PrefKey);

				//Save settings to database as a backup in case the preferences are cleared or lost.
				List<Config> S = await configService.GetSettingsListAsync();
				foreach (Config item in S)
				{
					switch (item.ConfigId)
					{
						case 2:
							item.StringValue = Config_FirstRun.ToShortDateString();
							break;
						case 3:
							item.IntValue = Config_CurrentProject;
							break;
						case 4:
							item.IntValue = Config_CurrentPhase;
							break;
						case 5:
							item.StringValue = Config_WorkCompRate.ToString();
							break;
						case 6:
							item.StringValue = Config_EstimateEmployerTaxes.ToString();
							break;
						case 7:
							item.StringValue = Config_ProfitRate.ToString();
							break;
						case 8:
							item.StringValue = Config_OverHeadRate.ToString();
							break;
						case 9:
							item.IntValue = Config_IsAdmin;
							break;
						case 10:
							item.IntValue = Config_DaysInPayPeriod;
							break;
						case 11:
							item.IntValue = ConfigIntPayDayOfWeek;
							break;
						case 13:
							item.IntValue = ConfigIntAppTheme;
							break;
						case 14:
							item.IntValue = (int)(Config_UseNotifications ? 1 : 0);
							break;
						case 15:
							item.StringValue = Config_NotificationHours.ToString();
							break;
					}
				}

				if (IsAdmin)
				{
					Preferences.Set("FirstRun", Config_FirstRun.ToShortDateString(), PrefSetting.PrefKey);
				}
				// Use the AppTheme setting
				Application.Current!.UserAppTheme = (AppTheme)Config_AppTheme;

#if !WINDOWS
				// Dont use notifications on windows
				UseNotifications = Config_UseNotifications;
#endif
				if (await configService.SaveConfigAsync())
				{
					await AppShell.DisplayToastAsync("Saved Settings");
				}
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "SettingsBasicPageViewModel.OnAppearing");
			}
			finally
			{
				Loading = false;
			}
		}

		private void ExtractAlarmFromString(string alarmString)
		{
			if (string.IsNullOrEmpty(alarmString))
				return;
			string[] parts = alarmString.Split('.');
			if (parts.Length == 2 &&
				int.TryParse(parts[0], out int hours) &&
				int.TryParse(parts[1], out int minutes))
			{
				SelectedHours = hours;
				SelectedMinutes = minutes;
			}
		}

		[RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}
	}
}
