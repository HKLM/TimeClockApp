using System.Reflection;

namespace TimeClockApp;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            TimeCardDataStore data = new();
            await data.GetCurrentProjectIdAsync();
            await data.GetCurrentPhaseIdAsync();
        }
        catch (AggregateException ax)
        {
            string z = FlattenAggregateException.ShowAggregateExceptionForPopup(ax, "AppShell");
            await App.AlertSvc!.ShowAlertAsync("Exception", z).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.WriteLine("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "AppShell");
            await App.AlertSvc!.ShowAlertAsync("Exception", ex.Message + "\n" + ex.InnerException).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Display app version on flyout footer
    /// </summary>
    public string AppVersion
    {
        get { return GetBuildDate(Assembly.GetExecutingAssembly()); }
    }

    private static string GetBuildDate(Assembly assembly)
    {
        AssemblyInformationalVersionAttribute attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return attribute?.InformationalVersion ?? default;
    }
}
