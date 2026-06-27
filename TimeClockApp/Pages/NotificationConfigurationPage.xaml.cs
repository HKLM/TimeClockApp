namespace TimeClockApp.Pages;

public partial class NotificationConfigurationPage : ContentPage
{
	protected readonly NotificationConfigurationViewModel viewModel;
	public NotificationConfigurationPage(NotificationConfigurationViewModel ViewModel)
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
			Log.WriteLine($"EXCEPTION ERROR\n{ex.Message}\n{ex.InnerException}", "EditTimeCard");
		}
	}

}