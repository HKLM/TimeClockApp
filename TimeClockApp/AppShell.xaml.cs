using System.Reflection;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
#nullable enable

namespace TimeClockApp;

public partial class AppShell : Shell
{
	protected readonly AppViewModel viewModel;
	private static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
	private static string? cachedAppVersion;
	private readonly INotificationService _notificationService;

	public AppShell(AppViewModel viewModel, INotificationService notificationService)
	{
		InitializeComponent();
		RegisterRoutes();
		this.viewModel = viewModel;
		BindingContext = this.viewModel;
		_notificationService = notificationService;
#if !WINDOWS
		_notificationService.NotificationReceived += ShowCustomAlertFromNotification;
		_notificationService.NotificationActionTapped += Current_NotificationActionTapped;
#endif
	}

	private void RegisterRoutes()
	{
		Routing.RegisterRoute("EditTimeCard", typeof(EditTimeCard));
		Routing.RegisterRoute("ChangeStartTime", typeof(ChangeStartTime));
		Routing.RegisterRoute("EditExpensePage", typeof(EditExpensePage));
		Routing.RegisterRoute("EditExpenseTypePage", typeof(EditExpenseTypePage));
		Routing.RegisterRoute("TeamEmployeesPage", typeof(TeamEmployeesPage));
		Routing.RegisterRoute("PayrollDetailPage", typeof(PayrollDetailPage));
		Routing.RegisterRoute("InvoiceDetailTimecards", typeof(InvoiceDetailTimecards));
		Routing.RegisterRoute("InvoiceDetailExpenses", typeof(InvoiceDetailExpenses));
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		viewModel.OnAppearing();
	}

	/// <summary>
	/// Display app version on flyout footer
	/// </summary>
	public string AppVersion
	{
		get
		{
			cachedAppVersion ??= GetBuildDate(ExecutingAssembly);
			return cachedAppVersion;
		}
	}

	private static string GetBuildDate(Assembly assembly)
	{
		AssemblyInformationalVersionAttribute? attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
		return attribute?.InformationalVersion ?? string.Empty;
	}

	/// <summary>
	/// Displays a toast notification
	/// </summary>
	/// <param name="message">The message to display</param>
	/// <returns></returns>
	public static async Task DisplayToastAsync(string message, bool UseLongToastDuration = false)
	{
		// Toast is currently not working in MCT on Windows
		if (OperatingSystem.IsWindows())
			return;
		ToastDuration duration = UseLongToastDuration ? ToastDuration.Long : ToastDuration.Short;
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		IToast toast = Toast.Make(message, duration, 14);
		await toast.Show(cancellationTokenSource.Token);
	}

	//public static async Task DisplaySnackbarAsync(string message)
	//{
	//	CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

	//	var snackbarOptions = new SnackbarOptions
	//	{
	//		BackgroundColor = Color.FromArgb("#FF3300"),
	//		TextColor = Colors.White,
	//		ActionButtonTextColor = Colors.Yellow,
	//		CornerRadius = new CornerRadius(0),
	//		Font = Font.SystemFontOfSize(18),
	//		ActionButtonFont = Font.SystemFontOfSize(14)
	//	};

	//	var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);
	//	await snackbar.Show(cancellationTokenSource.Token);
	//}

	private void ShowCustomAlertFromNotification(NotificationEventArgs e)
	{
		if (e.Request is null) return;

		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await DisplayAlertAsync(e.Request.Title, e.Request.Description, "OK").ConfigureAwait(false);
		});
	}

	private async void Current_NotificationActionTapped(NotificationActionEventArgs e)
	{
		if (this.viewModel.Loading) return;

		try
		{
			this.viewModel.Loading = true;

			if (e.IsDismissed) return;

			if (e.IsTapped && e.Request is not null)
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await DisplayAlertAsync(e.Request?.Title, e.Request?.Description, "OK").ConfigureAwait(false);
				});

				//_notificationService.Cancel(e.Request.NotificationId);
				if (Shell.Current.CurrentPage is not TimeCardPage)
					await Shell.Current.GoToAsync("//TimeCardPage");

				return;
			}
		}
		finally
		{
			this.viewModel.Loading = false;
		}
	}
}
