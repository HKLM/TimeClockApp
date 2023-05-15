namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ProjectHome : ContentPage
{
    protected readonly ProjectHomeViewModel viewModel;

    public ProjectHome(ProjectHomeViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Task viewmod = viewModel.OnAppearingAsync();
        await viewmod;
    }
}