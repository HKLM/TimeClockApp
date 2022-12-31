namespace TimeClockApp.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditProjectPage : ContentPage
{
    public EditProjectPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.OnAppearing();
    }

    private void ProjectNameBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry box = (Entry)sender;
        if (box != null)
        {
            if (string.IsNullOrEmpty(box.Text))
                buttonNew.IsEnabled = false;
            else
                buttonNew.IsEnabled = box.Text.Length != 0;
        }
    }

    private void ProjectPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker P = (Picker)sender;
        if (P != null)
        {
            buttonSave.IsEnabled = P.SelectedIndex > -1;
            buttonDelete.IsEnabled = P.SelectedIndex > -1;
        }
    }
}