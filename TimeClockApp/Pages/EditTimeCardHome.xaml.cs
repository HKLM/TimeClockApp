using TimeClockApp.Models;

namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditTimeCardHome : ContentPage
{
    public EditTimeCardHome()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //clear the selectedItem if returning from EditTimeCard page
        if (ItemsListView.SelectedItem != null)
            ItemsListView.SelectedItem = null;

        viewModel.OnAppearing();
    }

    private async void ItemsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is TimeCard item)
        {
            await Shell.Current.GoToAsync($"EditTimeCard?id={item.TimeCardId}");
            viewModel.OnAppearing();
        }
    }
}