namespace TimeClockApp.Pages;


public partial class TimeCardPage : ContentPage
{
	protected readonly TimeCardPageViewModel viewModel;

	public TimeCardPage(TimeCardPageViewModel ViewModel)
	{
		InitializeComponent();
		BindingContext = viewModel = ViewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.OnAppearing();
	}

	private async Task HandleTimeCardNavigationAsync(int id, string route)
	{
		if (id == 0)
		{
			await App.AlertSvc!.ShowAlertAsync("NOTICE", "There is no TimeCard to edit.\n\nYou must first create a Time Card before it can be edited.").ConfigureAwait(false);
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

	private async void CardTime_Clicked(object sender, EventArgs e)
	{
		if (sender is SwipeItem swipeItem && int.TryParse(swipeItem.CommandParameter?.ToString(), out int id))
			await HandleTimeCardNavigationAsync(id, "ChangeStartTime");
		else if (sender is MenuFlyoutItem itemClicked && int.TryParse(itemClicked.CommandParameter?.ToString(), out int id2))
			await HandleTimeCardNavigationAsync(id2, "ChangeStartTime");
	}

	private async void TeamPageToolbarButton_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("TeamEmployeesPage");
	}

	private async void CardEdit_Clicked(object sender, EventArgs e)
	{
		if (sender is SwipeItem swipeItem && int.TryParse(swipeItem.CommandParameter?.ToString(), out int id2))
			await HandleTimeCardNavigationAsync(id2, "EditTimeCard");
		else if (sender is MenuFlyoutItem itemClicked && int.TryParse(itemClicked.CommandParameter?.ToString(), out int id))
			await HandleTimeCardNavigationAsync(id, "EditTimeCard");
	}

	private async void SwipeRightMulti_Clicked(object sender, EventArgs e)
	{
		if (sender is SwipeItem swipeItem && int.TryParse(swipeItem.CommandParameter?.ToString(), out int id))
			await Shell.Current.GoToAsync($"EditMultiTimeCards?id={id}");
		else if (sender is MenuFlyoutItem itemClicked && int.TryParse(itemClicked.CommandParameter?.ToString(), out int id2))
			await Shell.Current.GoToAsync($"EditMultiTimeCards?id={id2}");
	}
}