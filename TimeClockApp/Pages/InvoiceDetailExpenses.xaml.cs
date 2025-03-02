namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class InvoiceDetailExpenses : ContentPage
{
    protected readonly InvoiceDetailExpensesViewModel viewModel;

    public InvoiceDetailExpenses(InvoiceDetailExpensesViewModel ViewModel)
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