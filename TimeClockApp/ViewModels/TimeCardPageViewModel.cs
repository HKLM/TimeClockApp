using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
#nullable enable

namespace TimeClockApp.ViewModels
{
	public partial class TimeCardPageViewModel(TimeCardService service, INotificationService notificationService) : BaseViewModel
	{
		protected readonly TimeCardService cardService = service;
		private readonly INotificationService _notificationService = notificationService;
		private int _tapCount;

		[ObservableProperty]
		public partial ObservableCollection<TimeCard> TimeCards { get; set; } = new();

		[ObservableProperty]
		public partial string ErrorMessage { get; set; } = string.Empty;

		[ObservableProperty]
		public partial ObservableCollection<Project> ProjectList { get; set; } = new();

		[ObservableProperty]
		public partial Project? SelectedProject { get; set; } = null;
		partial void OnSelectedProjectChanging(global::TimeClockApp.Shared.Models.Project? value)
		{
			if (value != null && SelectedProject != null && value.ProjectId != SelectedProject.ProjectId)
				Task.Run(() => cardService.SaveCurrentProjectAsync(value.ProjectId).ContinueWith(task =>
				{
					if (task.Exception != null)
					{
						Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "TimeCardPageViewModel.OnSelectedProjectChanging");
					}
				}));
		}

		[ObservableProperty]
		public partial ObservableCollection<Phase> PhaseList { get; set; } = new();

		[ObservableProperty]
		public partial Phase? SelectedPhase { get; set; } = null;
		partial void OnSelectedPhaseChanging(global::TimeClockApp.Shared.Models.Phase? value)
		{
			if (value != null && SelectedPhase != null && value.PhaseId != SelectedPhase.PhaseId)
				Task.Run(() => cardService.SaveCurrentPhaseAsync(value.PhaseId).ContinueWith(task =>
				{
					if (task.Exception != null)
					{
						Log.WriteLine($"Caught exception: {task.Exception.Flatten().InnerException?.Message}", "TimeCardPageViewModel.OnSelectedPhaseChanging");
					}
				}));
		}

		[RelayCommand]
		public async Task OnAppearing()
		{
			if (Loading)
				return;

			Loading = true;
			HasError = false;

			try
			{
				await Task.Run(async () =>
				{
					List<Project> proList = await cardService.GetProjectsListAsync();
					ProjectList = proList.ToObservableCollection();
					List<Phase> phaseList = await cardService.GetPhaseListAsync();
					PhaseList = phaseList.ToObservableCollection();

					SelectedProject = cardService.GetCurrentProjectEntity();
					SelectedPhase = cardService.GetCurrentPhaseEntity();

					List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
					TimeCards = t.ToObservableCollection();
				});
			}
			catch (Exception e)
			{
				HasError = true;
				Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "TimeCardPageViewModel.OnAppearing");
				ErrorMessage = "An error occurred while loading time cards. Please try again.";
			}
			finally
			{
				Loading = false;
			}
		}

		[RelayCommand]
		private async Task ClockIn(TimeCard? card)
		{
			if (card != null && SelectedProject != null && SelectedPhase != null)
			{
				if (await cardService.EmployeeClockInAsync(card, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle))
				{
					List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
					TimeCards = t.ToObservableCollection();

					await MakeNotification(card);
				}
			}
		}

		[RelayCommand]
		private async Task ClockOutAsync(TimeCard? card)
		{
			if (card != null && await cardService.EmployeeClockOutAsync(card.TimeCardId))
			{
				List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
				TimeCards = t.ToObservableCollection();
			}
		}

		[RelayCommand]
		private async Task ClockAll_InAsync()
		{
			if (TimeCards is null || SelectedProject is null || SelectedPhase is null)
				return;

			foreach (TimeCard? clock in TimeCards.Where(item => item.TimeCard_Status != ShiftStatus.ClockedIn).ToList())
			{
				await MakeNotification(clock);
			}

			List<Task<bool>> clockInTasks = TimeCards
				.Where(item => item.TimeCard_Status != ShiftStatus.ClockedIn)
				.Select(item => cardService.EmployeeClockInAsync(item, SelectedProject.ProjectId, SelectedPhase.PhaseId, SelectedProject.Name, SelectedPhase.PhaseTitle))
				.ToList();

			await Task.WhenAll(clockInTasks);

			List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
			TimeCards = t.ToObservableCollection();
		}

		[RelayCommand]
		private async Task ClockAll_OutAsync()
		{
			if (TimeCards is null)
				return;

			List<Task<bool>> clockOutTasks = TimeCards
				.Where(item => item.TimeCard_Status == ShiftStatus.ClockedIn)
				.Select(item => cardService.EmployeeClockOutAsync(item.TimeCardId))
				.ToList();

			await Task.WhenAll(clockOutTasks);

			List<TimeCard> t = await cardService.GetLastTimeCardForAllEmployeesAsync();
			TimeCards = t.ToObservableCollection();
		}

		[RelayCommand]
		private void OnToggleHelpInfoBox()
		{
			HelpInfoBoxVisible = !HelpInfoBoxVisible;
		}

		private async Task MakeNotification(TimeCard timeCard)
		{
			// Notifications are not supported on WinUI
			if (DeviceInfo.Platform == DevicePlatform.WinUI) return;

			if (!UseNotifications || _notificationService == null) return;

			_tapCount++;
			int notificationId = 100 + _tapCount;
			string title = "Employee shift ends soon";
			List<string> list = [
				"",
				notificationId.ToString(),
				title
			];
			// No need to use NotificationSerializer, you can use your own one.
			string serializeReturningData = System.Text.Json.JsonSerializer.Serialize(list, TimeClockApp.Utilities.AppJsonContext.Default.ListString);

			NotificationRequest request = new()
			{
				NotificationId = notificationId,
				Title = title,
				Subtitle = timeCard.TimeCard_EmployeeName,
				Description = $"Employee {timeCard.TimeCard_EmployeeName} shift started at {timeCard.TimeCard_StartTime}.",
				BadgeNumber = _tapCount,
				ReturningData = serializeReturningData,
				//Image =
				//    {
				//        //ResourceName = "icon",
				//        Binary = imageBytes
				//    },
				CategoryType = NotificationCategoryType.Status,
				Android =
				{
					IconSmallName = { ResourceName = "calendar_clock_24px" },
					Color = { ResourceName = "colorPrimary" },
					Priority = Plugin.LocalNotification.Core.Models.AndroidOption.AndroidPriority.High
				},
			};

			// if not specified, default sound will be played.
#if ANDROID
			request.Sound = DeviceInfo.Platform == DevicePlatform.Android ? "reminder" : "";
#endif
			// if not specified, notification will show immediately.
			DateTime notifyDateTime;
#if !DEBUG
			notifyDateTime = DateTime.Now.AddHours(7).AddMinutes(45);
			//request.Schedule.NotifyAutoCancelTime = DateTime.Now.AddHours(8);
#else
			notifyDateTime = DateTime.Now.AddSeconds(10);
#endif
			request.Schedule.NotifyTime = notifyDateTime;

			try
			{
				if (await _notificationService.AreNotificationsEnabled() == false)
				{
					await _notificationService.RequestNotificationPermission();
				}

				//#if ANDROID

				//            var myAlarmManager = AlarmManager.FromContext(Android.App.Application.Context);
				//            var canScheduleExactAlarms = myAlarmManager?.CanScheduleExactAlarms() ?? false;

				//            if (!canScheduleExactAlarms)
				//            {
				//                var uri = Android.Net.Uri.Parse($"package:{Android.App.Application.Context.PackageName}");
				//                var intent = new Intent(Android.Provider.Settings.ActionRequestScheduleExactAlarm, uri);
				//                Android.App.Application.Context.StartActivity(intent);

				//                canScheduleExactAlarms = myAlarmManager?.CanScheduleExactAlarms() ?? false;
				//            }

				//#endif

				var ff = await _notificationService.Show(request);
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception);
			}
		}
	}
}
