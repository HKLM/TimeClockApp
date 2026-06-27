namespace TimeClockApp.Pages;


public partial class EditExpensePage : ContentPage
{
    public EditExpensePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}