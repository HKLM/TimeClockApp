namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class InvoiceDetailTimecards : ContentPage
{
    protected readonly InvoiceDetailTimecardsViewModel viewModel;

    public InvoiceDetailTimecards(InvoiceDetailTimecardsViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }
}