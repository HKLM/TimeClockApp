namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TeamEmployeesPage : ContentPage
{
    protected readonly TeamEmployeesViewModel viewModel;

    public TeamEmployeesPage(TeamEmployeesViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = ViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}
