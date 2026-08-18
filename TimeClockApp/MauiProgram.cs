using Plugin.LocalNotification;
using TimeClockApp.Shared;
using TimeClockApp.Shared.Interfaces;
#nullable enable

namespace TimeClockApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		MauiAppBuilder builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseLocalNotification()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Database context - created on-demand to defer initialization
		builder.Services.AddSingleton<DataBackendContext>();

		// Startup service for deferred initialization
		builder.Services.AddSingleton<IStartupService>(services =>
			new StartupServiceImpl(services.GetRequiredService<DataBackendContext>()));

		// Core services
		builder.Services.AddSingleton<IAlertService, AlertService>();
		builder.Services.AddSingleton<ISharedService, SharedService>();
		builder.Services.AddSingleton<IFileService, FileService>();

		builder.RegisterAppServices();
		builder.RegisterViewModels();

		builder.Services.AddSingleton<AppShell>();

		return builder.Build();
	}

	public static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder)
	{
		builder.Services.AddSingleton<TimeCardService>();
		builder.Services.AddSingleton<EditTimeCardService>();
		builder.Services.AddSingleton<PayrollService>();
		builder.Services.AddSingleton<ExpenseService>();
		builder.Services.AddTransient<UserManagerService>();
		builder.Services.AddSingleton<ReportPageService>();
		builder.Services.AddSingleton<InvoiceService>();
		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddSingleton<AppPageService>();
		return builder;
	}

	public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
	{
		builder.Services.AddTransient<TimeCardPageViewModel>();
		builder.Services.AddTransient<TimeCardManagerViewModel>();
		builder.Services.AddTransient<EditTimeCardViewModel>();
		builder.Services.AddTransient<ChangeStartTimeViewModel>();
		builder.Services.AddTransient<PayrollPageViewModel>();
		builder.Services.AddSingleton<PayrollDetailViewModel>();
		builder.Services.AddTransient<ExpenseViewModel>();
		builder.Services.AddTransient<EditExpenseTypeViewModel>();
		builder.Services.AddTransient<TeamEmployeesViewModel>();
		builder.Services.AddTransient<UserManagerViewModel>();
		builder.Services.AddTransient<ReportPageViewModel>();
		builder.Services.AddTransient<InvoiceViewModel>();
		builder.Services.AddTransient<InvoiceDetailExpensesViewModel>();
		builder.Services.AddTransient<InvoiceDetailTimecardsViewModel>();
		builder.Services.AddTransient<SettingsBasicPageViewModel>();
		builder.Services.AddTransient<ExportPageViewModel>();
		builder.Services.AddSingleton<AppViewModel>();

		return builder;
	}
}