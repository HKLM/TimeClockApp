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

}
