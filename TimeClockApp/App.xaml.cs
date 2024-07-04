using TimeClockApp.Shared;

namespace TimeClockApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class App : Microsoft.Maui.Controls.Application
{
    public static IServiceProvider Services { get; protected set; }
    public static IAlertService AlertSvc;

    public App(IServiceProvider provider)
    {
        InitializeComponent();

        SQLiteSetting.SQLiteDBPath = SQLiteSetting.GetSQLiteDBPath();
        FirstRun = true;
        Services = provider;
        AlertSvc = Services.GetService<IAlertService>();

        MainPage = new AppShell();
    }

#region HELPERS
    /// <summary>
    /// Determine if this is the apps 1st time running
    /// </summary>
    public static bool FirstRun { get; private set; } = true;
    public static void SetFirstRun(bool IsFirstRun) => FirstRun = IsFirstRun;

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
    public static int CurrentProjectId { get; set; } = 0;
    public static int CurrentPhaseId { get; set; } = 0;

#endregion HELPERS
}
