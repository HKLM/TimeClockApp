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

    private async void ItemsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e?.SelectedItem is TimeCard item)
        {
            await Shell.Current.GoToAsync($"EditTimeCard?id={item.TimeCardId}");
            ItemsListView.SelectedItem = null;
        }
    }
}