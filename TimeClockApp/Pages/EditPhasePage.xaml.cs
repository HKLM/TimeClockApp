namespace TimeClockApp.Pages;


public partial class EditPhasePage : ContentPage
{
    public EditPhasePage()
    {
        InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}