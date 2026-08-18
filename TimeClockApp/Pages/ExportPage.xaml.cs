namespace TimeClockApp.Pages;


public partial class ExportPage : ContentPage
{
	protected readonly ExportPageViewModel viewModel;
	public ExportPage(ExportPageViewModel ViewModel)
	{
		InitializeComponent();
		ProgressBarAnimationBehavior.Easing = Easing.CubicOut;
		BindingContext = viewModel = ViewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}