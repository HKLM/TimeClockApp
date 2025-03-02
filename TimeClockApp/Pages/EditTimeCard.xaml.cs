namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditTimeCard : ContentPage
{
    protected readonly EditTimeCardViewModel viewModel;

    public EditTimeCard(EditTimeCardViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        viewModel.OnDisappearing();
        base.OnDisappearing();
    }
}