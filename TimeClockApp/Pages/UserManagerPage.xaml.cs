namespace TimeClockApp.Pages;

public partial class UserManagerPage : ContentPage
{
    public UserManagerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }

    private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry box = (Entry)sender;
        if (box != null)
        {
            if (string.IsNullOrEmpty(box.Text))
                NewEmpButton.IsEnabled = false;
            else
                NewEmpButton.IsEnabled = box.Text.Length != 0;
        }
    }

    private void EmployeePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker P = (Picker)sender;
        if (P != null)
        {
            EditButton.IsEnabled = P.SelectedIndex > -1;
            DelButton.IsEnabled = P.SelectedIndex > -1;
        }
    }
}