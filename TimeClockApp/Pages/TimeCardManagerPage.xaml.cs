namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TimeCardManagerPage : ContentPage
{
    protected readonly TimeCardManagerViewModel viewModel;

    public TimeCardManagerPage(TimeCardManagerViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.OnAppearing();
    }

    private async void FlyGoEdit_Clicked(object sender, EventArgs e)
    {
        MenuFlyoutItem itemClicked = (MenuFlyoutItem)sender;
        if (itemClicked != null)
        {
            if (int.TryParse(itemClicked.CommandParameter.ToString(), out int i))
            {
                if (i == 0)
                {
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "There is no TimeCard to edit.\n\nYou must first create a Time Card before it can be edited.");
                    return;
                }
                else
                {
                    await Shell.Current.GoToAsync($"EditTimeCard?id={i}");
                }
            }
        }
    }

    private async void timeCardsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e == null || e?.CurrentSelection == null || e.CurrentSelection.FirstOrDefault() == null)
            return;
        int? current = (e.CurrentSelection.FirstOrDefault() as TimeCard)?.TimeCardId;
        if (current.HasValue)
        {
            await Shell.Current.GoToAsync($"EditTimeCard?id={current.Value}");
            timeCardsCollection.SelectedItem = null;
        }
    }
}