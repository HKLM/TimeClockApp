namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TimeCardPage : ContentPage
{
    protected readonly TimeCardPageViewModel viewModel;

    public TimeCardPage(TimeCardPageViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }

    private async void Swipeitemaction_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem != null)
        {
            if (int.TryParse(swipeItem.CommandParameter.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"ChangeStartTime?id={i}");
                await viewModel.OnAppearing();
            }
        }
    }

    private async void SwipeitemStart_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem != null)
        {
            if (int.TryParse(swipeItem.CommandParameter.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"EditTimeCard?id={i}");
                await viewModel.OnAppearing();
            }
        }
    }

    private async void TeamPageToolbarButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"TeamEmployeesPage");
        await viewModel.OnAppearing();
    }
}