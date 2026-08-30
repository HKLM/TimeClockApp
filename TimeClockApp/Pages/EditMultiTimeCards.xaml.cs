namespace TimeClockApp.Pages;

public partial class EditMultiTimeCards : ContentPage
{
	protected readonly EditMultiTimeCardsViewModel viewModel;

	public EditMultiTimeCards(EditMultiTimeCardsViewModel ViewModel)
	{
		InitializeComponent();
		BindingContext = viewModel = ViewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.OnAppearing();
	}
}