using System.Reflection;

namespace TimeClockApp;

#nullable enable

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AppShell : Shell
{
    protected readonly AppViewModel viewModel;
    private static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
    private static string? cachedAppVersion;

    public AppShell(AppViewModel viewModel)
    {
        InitializeComponent();
        RegisterRoutes();
        this.viewModel = viewModel;
        BindingContext = this.viewModel;
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
        Application.Current!.UserAppTheme = viewModel.GetThemeType();
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
}
