using TimeClockApp.Shared.Interfaces;

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
        builder.Services.AddSingleton<DataBackendContext>((services) => new());

        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<ISharedService, SharedService>();

        builder.Services.AddSingleton<TimeCardService>();
        builder.Services.AddTransient<TimeCardPageViewModel>();
        builder.Services.AddTransient<TimeCardPage>();

        builder.Services.AddSingleton<EditTimeCardService>();
        builder.Services.AddTransient<EditTimeCardViewModel>();
        builder.Services.AddTransient<EditTimeCard>();

        builder.Services.AddTransient<ChangeStartTimeViewModel>();
        builder.Services.AddTransient<ChangeStartTime>();

        builder.Services.AddSingleton<PayrollService>();
        builder.Services.AddTransient<PayrollPageViewModel>();
        builder.Services.AddTransient<PayrollPage>();

        builder.Services.AddTransient<PayrollDetailViewModel>();
        builder.Services.AddTransient<PayrollDetailPage>();

        builder.Services.AddSingleton<ExpenseService>();
        builder.Services.AddTransient<ExpenseViewModel>();
        builder.Services.AddSingleton<ExpensePage>();

        builder.Services.AddTransient<EditExpenseTypeViewModel>();
        builder.Services.AddTransient<EditExpenseTypeTab>();

        builder.Services.AddTransient<EditExpenseTypePage>();

        builder.Services.AddTransient<TeamEmployeesViewModel>();
        builder.Services.AddTransient<TeamEmployeesPage>();

        builder.Services.AddTransient<TimeCardManagerViewModel>();
        builder.Services.AddTransient<TimeCardManagerPage>();

        builder.Services.AddSingleton<ReportPageService>();
        builder.Services.AddTransient<ReportPageViewModel>();
        builder.Services.AddTransient<ReportPage>();

        builder.Services.AddSingleton<InvoiceService>();
        builder.Services.AddTransient<InvoiceViewModel>();
        builder.Services.AddSingleton<InvoicePage>();

        builder.Services.AddTransient<InvoiceDetailExpensesViewModel>();
        builder.Services.AddTransient<InvoiceDetailTimecardsViewModel>();

        builder.Services.AddSingleton<AppPageService>();
        builder.Services.AddTransient<AppViewModel>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
