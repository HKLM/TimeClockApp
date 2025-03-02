namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PayrollDetailPage : ContentPage
{
    protected readonly PayrollDetailViewModel viewModel;
    private double _width = 0;
    private double _height = 0;

    public PayrollDetailPage(PayrollDetailViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = viewModel = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await viewModel.OnAppearing();

        _width = this.Width;
        _height = this.Height;
        if (_width != 0)
        {
            LandscapeItemsVisibility(_width > _height);
        }
    }

    private async void SwipeEdit_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem != null)
        {
            if (int.TryParse(swipeItem.CommandParameter?.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"EditTimeCard?id={i}");
            }
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height); // Important!

        if (width != _width || height != _height)
        {
            _width = width;
            _height = height;

            LandscapeItemsVisibility(width > height);
        }
    }

    protected void LandscapeItemsVisibility(bool visible)
    {
        viewModel?.ChangeDisplayLandscapeModeCommand.Execute(visible);
    }

    private async void FlyItemEditCard_Clicked(object sender, EventArgs e)
    {
        MenuFlyoutItem swipeItem = (MenuFlyoutItem)sender;
        if (swipeItem != null)
        {
            if (int.TryParse(swipeItem.CommandParameter?.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"EditTimeCard?id={i}");
            }
        }
    }
}