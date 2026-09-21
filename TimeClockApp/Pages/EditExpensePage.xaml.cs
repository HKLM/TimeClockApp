namespace TimeClockApp.Pages;


public partial class EditExpensePage : ContentPage
{
	protected readonly EditExpensePageViewModel viewModel;
	public EditExpensePage(EditExpensePageViewModel ViewModel)
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