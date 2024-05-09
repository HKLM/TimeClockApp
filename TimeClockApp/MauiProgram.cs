using TimeClockApp.FileHelper;

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
        builder.Services.AddTransient<DataBackendContext>((services) => new DataBackendContext(SQLiteSetting.SQLiteDBPath));
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<TimeCardService>();
        builder.Services.AddTransient<TimeCardPageViewModel>();
        builder.Services.AddSingleton<TimeCardPage>();

        builder.Services.AddSingleton<EditTimeCardService>();
        builder.Services.AddTransient<EditTimeCardViewModel>();
        builder.Services.AddSingleton<EditTimeCard>();

        builder.Services.AddTransient<ChangeStartTimeViewModel>();
        builder.Services.AddSingleton<ChangeStartTime>();

        builder.Services.AddSingleton<PayrollService>();
        builder.Services.AddTransient<PayrollPageViewModel>();
        builder.Services.AddTransient<PayrollPage>();

        builder.Services.AddTransient<PayrollDetailViewModel>();
        builder.Services.AddTransient<PayrollDetailPage>();

        builder.Services.AddSingleton<ExpenseService>();
        builder.Services.AddTransient<ExpenseViewModel>();
        builder.Services.AddSingleton<ExpensePage>();

        builder.Services.AddSingleton<UserManagerService>();
        builder.Services.AddTransient<TeamEmployeesViewModel>();
        builder.Services.AddSingleton<TeamEmployeesPage>();

        builder.Services.AddTransient<EditTimeCardHomeViewModel>();
        builder.Services.AddSingleton<EditTimeCardHome>();

        return builder.Build();
    }
}
