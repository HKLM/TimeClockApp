namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ChangeStartTime : ContentPage
{
    protected readonly ChangeStartTimeViewModel viewModel;

    public ChangeStartTime(ChangeStartTimeViewModel ViewModel)
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
            Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "ChangeStartTime");
        }
    }
}