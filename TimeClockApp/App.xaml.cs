using TimeClockApp.Services;

namespace TimeClockApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class App : Microsoft.Maui.Controls.Application
{
    /// <summary>
    /// File name (only) of the SQLite Database.
    /// </summary>
    /// <remarks>Does not include path.</remarks>
    public const string SQLiteFileName = "TimeClockAppDB-01.db3";

    //For the Alart Popup from ViewModel support
    public static IServiceProvider Services;
    public static IAlertService AlertSvc;

    public App(IServiceProvider provider)
    {
        InitializeComponent();

        FirstRun = true;
        Services = provider;
        AlertSvc = Services.GetService<IAlertService>();

        MainPage = new AppShell();
    }

    #region HELPERS

    /// <summary>
    /// Get the full file path to SQLite Database
    /// </summary>
    private static void GetSQLiteDBPath()
    {
        FileHelperService fhs = new();
        SQLiteDBPath = fhs.GetDBPath(SQLiteFileName);
    }

    private static string sQLite_DBPath;
    public static string SQLiteDBPath
    {
        get
        {
            if (sQLite_DBPath == null || sQLite_DBPath == "" || sQLite_DBPath == string.Empty)
                GetSQLiteDBPath();

            return sQLite_DBPath;
        }
        private set => sQLite_DBPath = value;
    }

    /// <summary>
    /// Determin if this is the apps 1st time running
    /// </summary>
    public static bool FirstRun { get; set; } = true;
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
    public static double CurrentWorkCompRate { get; set; } = 0;
    //TODO
    public static double EstimatedEmployerPayrollTaxRate { get; set; } = 0;
    //TODO
    public static int OverHeadRate { get; set; } = 0;
    //TODO
    //public static int ProfitRate { get; set; } = 0;


    #endregion HELPERS

}
