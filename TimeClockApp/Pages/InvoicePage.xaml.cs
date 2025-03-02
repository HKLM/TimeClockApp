namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class InvoicePage : ContentPage
{
    protected readonly InvoiceViewModel viewModel;
    public InvoicePage(InvoiceViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
        ViewModel.Loading = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.OnAppearing();
    }
}