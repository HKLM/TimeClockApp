namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ReportWeekPage : ContentPage
{
    private double _width = 0;
    private double _height = 0;

    public ReportWeekPage()
    {
        InitializeComponent();
    }

#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
    protected override async void OnAppearing()
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates
    {
        base.OnAppearing();
        await viewModel.OnAppearingAsync();
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
            if (int.TryParse(swipeItem.CommandParameter.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"EditTimeCard?id={i}");
                if (!viewModel.RefreshCardsCommand.IsRunning)
                    await viewModel.RefreshCardsCommand.ExecuteAsync(null);
            }
        }
    }

    private async void SwipeChangeTime_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem != null)
        {
            if (int.TryParse(swipeItem.CommandParameter.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"ChangeStartTime?id={i}");
                if (!viewModel.RefreshCardsCommand.IsRunning)
                    await viewModel.RefreshCardsCommand.ExecuteAsync(null);
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
}