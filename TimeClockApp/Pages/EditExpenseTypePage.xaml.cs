namespace TimeClockApp.Pages;

/// <summary>
/// Popup type page for EditExpenseType
/// </summary>
[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditExpenseTypePage : ContentPage
{
    protected readonly EditExpenseTypeViewModel viewModel;

    public EditExpenseTypePage(EditExpenseTypeViewModel ViewModel)
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