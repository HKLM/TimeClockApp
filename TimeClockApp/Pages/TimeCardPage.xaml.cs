namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class TimeCardPage : ContentPage
{
    protected readonly TimeCardPageViewModel viewModel;

    public TimeCardPage(TimeCardPageViewModel ViewModel)
    {
        InitializeComponent();
        viewModel = ViewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await viewModel.OnAppearing();
        }
        catch (AggregateException ax)
        {
            TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax, "TimeCardPage");
        }
        catch (Exception ex)
        {
            Log.WriteLine("EXCEPTION ERROR\n" + ex.Message + "\n" + ex.InnerException, "TimeCardPage");
        }
    }

    private async void Swipeitemaction_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem != null)
        {
            if (int.TryParse(swipeItem.CommandParameter.ToString(), out int i))
            {
                if (i == 0)
                {
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "There is no TimeCard to edit.\n\nYou must first create a Time Card before it can be edited.");
                    return;
                }
                else
                {
                    await Shell.Current.GoToAsync($"ChangeStartTime?id={i}");
                }
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

    private async void TeamPageToolbarButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"TeamEmployeesPage");
    }

    private async void FlyItemStart_Clicked(object sender, EventArgs e)
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

    private async void FlyItemEditStart_Clicked(object sender, EventArgs e)
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
                    await Shell.Current.GoToAsync($"ChangeStartTime?id={i}");
                }
            }
        }
    }
}