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
        builder.Services.AddTransient<DataBackendContext>((services) => new());

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
        //builder.Services.AddTransient<PayrollDetailPage>();
        //builder.Services.AddSingleton<PayrollDetailViewModel>();
        builder.Services.AddSingleton<PayrollDetailPage>();

        builder.Services.AddSingleton<ExpenseService>();
        builder.Services.AddTransient<ExpenseViewModel>();
        builder.Services.AddSingleton<ExpensePage>();

        builder.Services.AddSingleton<EditExpenseTypeService>();
        builder.Services.AddTransient<EditExpenseTypeViewModel>();
        builder.Services.AddSingleton<EditExpenseTypePage>();

        builder.Services.AddSingleton<UserManagerService>();
        builder.Services.AddTransient<TeamEmployeesViewModel>();
        builder.Services.AddSingleton<TeamEmployeesPage>();

        builder.Services.AddTransient<TimeCardManagerViewModel>();
        builder.Services.AddSingleton<TimeCardManagerPage>();

        builder.Services.AddSingleton<ReportPageService>();
        builder.Services.AddTransient<ReportPageViewModel>();
        builder.Services.AddSingleton<ReportPage>();
        return builder.Build();
    }
}
