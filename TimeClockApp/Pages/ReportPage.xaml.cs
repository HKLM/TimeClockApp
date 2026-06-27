namespace TimeClockApp.Pages;


public partial class ReportPage : ContentPage
{
    protected readonly ReportPageViewModel viewModel;

    public ReportPage(ReportPageViewModel ViewModel)
	{
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

	protected override async void OnAppearing()
	{
        base.OnAppearing();
        await viewModel.OnAppearing();
    }
}