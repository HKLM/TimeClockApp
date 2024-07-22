namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ExpensePage : ContentPage
{
    protected readonly ExpenseViewModel viewModel;

    public ExpensePage(ExpenseViewModel ViewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = ViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await viewModel.OnAppearing();
    }

    private async void ExpenseEditAction_Clicked(object sender, EventArgs e)
    {
        SwipeItem swipeItem = (SwipeItem)sender;
        if (swipeItem?.CommandParameter != null)
        {
            if (int.TryParse(swipeItem.CommandParameter.ToString(), out int i))
            {
                await Shell.Current.GoToAsync($"EditExpensePage?id={i}");
            }
        }
    }

    private async void ArchiveButton_ClickedAsync(object sender, EventArgs e)
    {
        if (await App.AlertSvc.ShowConfirmationAsync("Notice", "Do you want to Archive all currently displayed expenses?"))
            viewModel?.ArchiveExpenseListCommand.Execute(null);
    }

    private void ShowArchivedChkBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        viewModel?.ToggleShowArchivedCommand.Execute(e.Value);
    }
}