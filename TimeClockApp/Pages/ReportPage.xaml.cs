namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ReportPage : ContentPage
{
    protected readonly ReportPageViewModel viewModel;

    public ReportPage(ReportPageViewModel ViewModel)
	{
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}