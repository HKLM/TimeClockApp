[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TimeClockApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<IAlertService, AlertService>();

        builder.Services.AddSingleton<TimeCardPageViewModel>();
        builder.Services.AddSingleton<TimeCardPage>();
        builder.Services.AddTransient<TimeCardService>();

        builder.Services.AddSingleton<ReportWeekViewModel>();
        builder.Services.AddSingleton<ReportWeekPage>();
        builder.Services.AddTransient<ReportDataService>();

        builder.Services.AddSingleton<ProjectHomeViewModel>();
        builder.Services.AddSingleton<ProjectHome>();
        builder.Services.AddTransient<ProjectDetailService>();

        builder.Services.AddSingleton<EditTimeCardViewModel>();
        builder.Services.AddSingleton<EditTimeCard>();
        builder.Services.AddTransient<EditTimeCardService>();

        builder.Services.AddSingleton<ChangeStartTimeViewModel>();
        builder.Services.AddSingleton<ChangeStartTime>();

        builder.Services.AddSingleton<ExpenseViewModel>();
        builder.Services.AddSingleton<ExpensePage>();
        builder.Services.AddTransient<ExpenseService>();

        builder.Services.AddSingleton<TeamEmployeesViewModel>();
        builder.Services.AddSingleton<TeamEmployeesPage>();
        builder.Services.AddTransient<UserManagerService>();

        return builder.Build();
    }
}
