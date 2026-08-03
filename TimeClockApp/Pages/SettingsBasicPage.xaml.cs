namespace TimeClockApp.Pages;

public partial class SettingsBasicPage : ContentPage
{
    protected readonly SettingsBasicPageViewModel viewModel;

    public SettingsBasicPage(SettingsBasicPageViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await viewModel.OnAppearing();
        }
        catch (Exception ex)
        {
            Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "SettingsPage");
        }
    }

    //protected override async void OnDisappearing()
    //{
    //    await viewModel.SaveSettingsCommand.ExecuteAsync(null);
    //    base.OnDisappearing();
    //}
}