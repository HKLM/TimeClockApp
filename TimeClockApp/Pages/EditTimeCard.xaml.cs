namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditTimeCard : ContentPage
{
    public EditTimeCard()
    {
        InitializeComponent();
    }

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel?.OnAppearingAsync();
    }
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
}