#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class NotificationConfigurationViewModel : BaseViewModel
	{
		protected readonly AppPageService service = new();

		public NotificationConfigurationViewModel()
		{
			PickerHours ??= Enumerable.Range(0, 24).ToObservableCollection();
			PickerMinutes ??= [];
			for (int i = 0; i < 60; i += 5)
			{
				PickerMinutes.Add(i);
			}
		}

		[ObservableProperty]
		public partial int SelectedHours { get; set; } = 0;
		[ObservableProperty]
		public partial int SelectedMinutes { get; set; } = 0;
		[ObservableProperty]
		public partial ObservableCollection<int> PickerHours { get; set; }
		[ObservableProperty]
		public partial ObservableCollection<int> PickerMinutes { get; set; } = new();

		[RelayCommand]
		public async Task OnAppearing()
		{
			ExtractAlarmFromString((await service.GetNotificationHoursSettingAsync()).ToString("0.00"));
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
		private async Task SaveAsync()
		{
			string alarmString = $"{SelectedHours}.{SelectedMinutes:00}";
			if (double.TryParse(alarmString, out double alarmValue))
			{
				await service.SaveNotificationHoursSettingAsync(alarmValue);
				AppViewModel.NotificationHours = alarmValue;
			}
		}

	}
}
