namespace TimeClockApp;

#nullable enable
[XamlCompilation(XamlCompilationOptions.Compile)]
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

#if WINDOWS
    protected override Window CreateWindow(IActivationState? activationState) => new(_appShell) { Width = 800 };
#else
    protected override Window CreateWindow(IActivationState? activationState) => new(_appShell);
#endif

#region HELPERS
    private static ViewModels.EntityMonitor GetEntityMonitor = new();
    public static bool NoticeProjectHasChanged
    {
        get => GetEntityMonitor.NoticeProjectHasChanged;
        set => GetEntityMonitor.NoticeProjectHasChanged = value;
    }
    public static bool NoticePhaseHasChanged
    {
        get => GetEntityMonitor.NoticePhaseHasChanged;
        set => GetEntityMonitor.NoticePhaseHasChanged = value;
    }
    public static bool NoticeUserHasChanged
    {
        get => GetEntityMonitor.NoticeUserHasChanged;
        set => GetEntityMonitor.NoticeUserHasChanged = value;
    }

    // Cached for quick access
    public static int? CurrentProjectId { get; set; } = null;
    public static int? CurrentPhaseId { get; set; } = null;

#endregion HELPERS
}
