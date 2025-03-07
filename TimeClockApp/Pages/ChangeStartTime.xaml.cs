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
        await viewModel.OnAppearing();
    }
}