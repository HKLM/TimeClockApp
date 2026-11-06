namespace TimeClockApp;

#nullable enable

public partial class App : Application
{
	readonly AppShell _appShell;
	public static IServiceProvider? Services { get; protected set; }
	public static IAlertService? AlertSvc;

	public App(IServiceProvider provider)
	{
		InitializeComponent();

		Services = provider;
		AlertSvc = Services.GetService<IAlertService>();
		_appShell = Services.GetService<AppShell>()!;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Trigger deferred initialization before UI is loaded
		// Use MainThread to ensure it runs on the correct thread
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			IStartupService? startupService = Services?.GetService<IStartupService>();
			if (startupService != null && !startupService.IsInitialized)
			{
				// Fire and forget - don't block the UI thread
				_ = startupService.InitializeAsync(CancellationToken.None);
			}
		});

		// Create window with the app shell
#if WINDOWS
		var window = new Window(_appShell) { Width = 800 };
#else
		var window = new Window(_appShell);
#endif

		return window;
	}
}
