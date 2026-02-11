namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TimeCardPage : ContentPage
{
    private const string NoTimeCardMessage = "There is no TimeCard to edit.\n\nYou must first create a Time Card before it can be edited.";
    protected readonly TimeCardPageViewModel viewModel;

    public TimeCardPage(TimeCardPageViewModel ViewModel)
    {
        InitializeComponent();
        viewModel = ViewModel;
        BindingContext = viewModel;
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
            Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "TimeCardPage");
        }
    }

    private async Task HandleTimeCardNavigationAsync(int id, string route)
    {
        if (id == 0)
        {
            await App.AlertSvc!.ShowAlertAsync("NOTICE", NoTimeCardMessage).ConfigureAwait(false);
            return;
        }

        try
        {
            await Shell.Current.GoToAsync($"{route}?id={id}");
        }
        catch (Exception e)
        {
            Log.WriteLine($"{e.Message}\n  -- {e.Source}\n  -- {e.InnerException}", "TimeCardPage.HandleTimeCardNavigationAsync");
        }
    }

    private async void Swipeitemaction_Clicked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || !int.TryParse(swipeItem.CommandParameter?.ToString(), out int id))
            return;

        await HandleTimeCardNavigationAsync(id, "ChangeStartTime");
    }

    private async void SwipeitemStart_Clicked(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem || !int.TryParse(swipeItem.CommandParameter?.ToString(), out int id))
            return;

        await HandleTimeCardNavigationAsync(id, "EditTimeCard");
    }

    private async void TeamPageToolbarButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("TeamEmployeesPage");
    }

    private async void FlyItemStart_Clicked(object sender, EventArgs e)
    {
        if (sender is not MenuFlyoutItem itemClicked || !int.TryParse(itemClicked.CommandParameter?.ToString(), out int id))
            return;

        await HandleTimeCardNavigationAsync(id, "EditTimeCard");
    }

    private async void FlyItemEditStart_Clicked(object sender, EventArgs e)
    {
        if (sender is not MenuFlyoutItem itemClicked || !int.TryParse(itemClicked.CommandParameter?.ToString(), out int id))
            return;

        await HandleTimeCardNavigationAsync(id, "ChangeStartTime");
    }
}