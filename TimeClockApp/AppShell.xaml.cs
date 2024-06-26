﻿using System.Reflection;

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
        Routing.RegisterRoute("TeamEmployeesPage", typeof(TeamEmployeesPage));
        Routing.RegisterRoute("PayrollDetailPage", typeof(PayrollDetailPage));
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
