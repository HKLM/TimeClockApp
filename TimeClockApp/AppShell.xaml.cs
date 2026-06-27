using System.Reflection;
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
		Routing.RegisterRoute("PageNotificationConfiguration", typeof(NotificationConfigurationPage));
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
