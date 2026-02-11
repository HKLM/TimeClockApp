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

        try
        {
            await viewModel.OnAppearing();
        }
        catch (Exception ex)
        {
            Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "EditTimeCard");
        }
    }

    protected override void OnDisappearing()
    {
        viewModel.OnDisappearing();
        base.OnDisappearing();
    }
}