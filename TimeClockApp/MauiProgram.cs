using CommunityToolkit.Maui;

using TimeClockApp.Services;

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
            })
            .Services.AddSingleton<IAlertService, AlertService>();

        return builder.Build();
    }
}
