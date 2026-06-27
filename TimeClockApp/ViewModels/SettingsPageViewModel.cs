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
		private static readonly char[] TrimChars = { ' ', '$', ',' };

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
			HasError = false;
			if (item == null) return;
			// FirstRun
			if (item.ConfigId == 2 && !IsAdmin)
			{
				HasError = true;
				Log.WriteLine("Access denied: Only administrators can modify the FirstRun setting.", "SettingsPageViewModel.SaveSetting");
				await App.AlertSvc!.ShowAlertAsync("Access denied", "Only administrators can modify the FirstRun setting.").ConfigureAwait(false);
				return;
			}
			// Validate ProjectID
			if (item.ConfigId == 3)
			{
				if (item.IntValue.HasValue && !configService.IsValidProject(item.IntValue.Value))
				{
					HasError = true;
					Log.WriteLine($"Invalid Project ID: {item.IntValue.Value} does not exist.", "SettingsPageViewModel.SaveSetting");
					await App.AlertSvc!.ShowAlertAsync("Invalid Project ID", $"Project ID {item.IntValue.Value} does not exist. Please enter a valid Project ID.").ConfigureAwait(false);
					return;
				}
			}
			// Validate PhaseID
			if (item.ConfigId == 4)
			{
				if (item.IntValue.HasValue && !configService.IsValidPhase(item.IntValue.Value))
				{
					HasError = true;
					Log.WriteLine($"Invalid Phase ID: {item.IntValue.Value} does not exist.", "SettingsPageViewModel.SaveSetting");
					await App.AlertSvc!.ShowAlertAsync("Invalid Phase ID", $"Phase ID {item.IntValue.Value} does not exist. Please enter a valid Phase ID.").ConfigureAwait(false);
					return;
				}
			}
			// IsAdmin
			if (item.ConfigId == 9)
			{
				if (item.IntValue.HasValue)
				{
					bool bTest = IntToBool(item.IntValue.Value);
					item.IntValue = bTest ? 1 : 0;
				}
				else
				{
					HasError = true;
					await App.AlertSvc!.ShowAlertAsync("Error", $"IsAdmin requires a value 1 or 0. (1=True, 0=False)").ConfigureAwait(false);
					return;
				}
			}
			// AppTheme
			if (item.ConfigId == 13)
			{
				if (!item.IntValue.HasValue && !string.IsNullOrEmpty(item.StringValue))
				{
					var trimmed = item.StringValue.Trim(TrimChars);
					if (int.TryParse(trimmed, out int result))
					{
						item.IntValue = result;
						item.StringValue = null;
					}
				}
			}

			if (await configService.SaveConfigAsync(item))
			{
				// Save the AppTheme setting
				if (item.ConfigId == 13 && item.IntValue.HasValue)
				{
					Application.Current!.UserAppTheme = (AppTheme)item.IntValue!.Value;
				}
#if !WINDOWS
				// Dont use notifications on windows
				if (item.ConfigId == 14 && item.IntValue.HasValue)
				{
					UseNotifications = IntToBool(item.IntValue!.Value);
				}
#endif
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
