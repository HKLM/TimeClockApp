namespace TimeClockApp.Pages;

/// <summary>
/// Shell tab page for EditExpenseType
/// </summary>

public partial class EditExpenseTypeTab : ContentPage
{
    protected readonly EditExpenseTypeViewModel viewModel;

    public EditExpenseTypeTab(EditExpenseTypeViewModel ViewModel)
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