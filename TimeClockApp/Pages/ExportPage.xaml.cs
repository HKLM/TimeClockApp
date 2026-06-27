namespace TimeClockApp.Pages;


public partial class ExportPage : ContentPage
{
	public ExportPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}