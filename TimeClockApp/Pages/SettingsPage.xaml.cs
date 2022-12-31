namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}