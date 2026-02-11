namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PayrollPage : ContentPage
{
    protected readonly PayrollPageViewModel viewModel;
    public PayrollPage(PayrollPageViewModel ViewModel)
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
            Log.WriteLine($"{ex.Message}\n  -- {ex.Source}\n  -- {ex.InnerException}", "PayrollPage.OnAppearing");
        }
    }

    private async void TeamPageToolbarButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"TeamEmployeesPage");
    }

    private async void SwipeItem_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem?.CommandParameter != null)
        {
            if (int.TryParse(swipeItem.CommandParameter!.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"PayrollDetailPage?id={i}&start={DatePickerStart.Date:MM-dd-yyyy}&end={DatePickerEnd.Date:MM-dd-yyyy}");
            }
        }
    }
}