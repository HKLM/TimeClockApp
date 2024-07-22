using CommunityToolkit.Maui.Core.Extensions;
#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class EditExpenseTypeViewModel(EditExpenseTypeService service) : TimeStampViewModel, IQueryAttributable
    {
        protected readonly EditExpenseTypeService dataService = service;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { ExpenseId = i; }
            }
        }

        [ObservableProperty]
        private int expenseId = 0;
        partial void OnExpenseIdChanged(int value)
        {
            if (value > 0)
            {
                SelectedExpenseType = dataService.GetExpenseType(value);
            }
        }

        [ObservableProperty]
        private ObservableCollection<ExpenseType> expenseTypeList = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableDeleteButton))]
        private ExpenseType? selectedExpenseType = new();
        partial void OnSelectedExpenseTypeChanged(global::TimeClockApp.Shared.Models.ExpenseType? oldValue, global::TimeClockApp.Shared.Models.ExpenseType? newValue)
        {
            if (newValue == null)
            {
                NewCategoryName = string.Empty;
            }
            else if (oldValue != newValue)
            {
                NewCategoryName = newValue.CategoryName;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAddButton))]
        private string newCategoryName = string.Empty;

        public bool EnableAddButton => !string.IsNullOrEmpty(NewCategoryName);
        public bool EnableDeleteButton => SelectedExpenseType != null;

        public async Task OnAppearing()
        {
            ExpenseTypeList ??= [];
            List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
            ExpenseTypeList = x.ToObservableCollection();
        }

        [RelayCommand]
        private async Task AddExpenseTypeAsync()
        {
            if (string.IsNullOrEmpty(NewCategoryName))
            {
                await App.AlertSvc.ShowAlertAsync("NOTICE", "ExpenseType must have a name.");
                return;
            }

            string newName = NewCategoryName.Trim();
            var r = dataService.AddExpenseTypeAsync(newName);
            int i = await r;
            if (i == 1)
            {
                SelectedExpenseType = null;
                List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                ExpenseTypeList = x.ToObservableCollection();
                NewCategoryName = string.Empty;
                await App.AlertSvc.ShowAlertAsync("NOTICE", "Saved");
            }
            else if (i == 2)
            {
                await App.AlertSvc.ShowAlertAsync("DUPLICATE", newName + " already exists in database. Failed to add new ExpenseType.");
            }
            else
                await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to add new ExpenseType");
        }

        [RelayCommand]
        private async Task EditExpenseTypeAsync()
        {
            if (SelectedExpenseType == null || string.IsNullOrEmpty(NewCategoryName))
                return;
            try
            {
                string newName = NewCategoryName.Trim();
                if (await dataService.UpdateExpenseTypeAsync(SelectedExpenseType.ExpenseTypeId, newName))
                {
                    SelectedExpenseType = null;
                    List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                    ExpenseTypeList = x.ToObservableCollection();

                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Saved");
                }
                else
                    await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to save ExpenseType");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DelExpenseTypeAsync()
        {
            if (SelectedExpenseType == null)
                return;

            try
            {
                System.Diagnostics.Debug.WriteLine("User is attempting to delete a ExpenseType record");
                if (await App.AlertSvc.ShowConfirmationAsync("CONFIRMATION", "Are you sure you want to Delete this ExpenseType?"))
                {
                    if (await dataService.DeleteExpenseTypeAsync(SelectedExpenseType.ExpenseTypeId))
                    {
                        SelectedExpenseType = null;
                        List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                        ExpenseTypeList = x.ToObservableCollection();

                        await App.AlertSvc.ShowAlertAsync("NOTICE", "Deleted");
                        //await Shell.Current.GoToAsync("..");
                    }
                    else
                        await App.AlertSvc.ShowAlertAsync("NOTICE", "Failed to delete ExpenseType");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private void OnToggleHelpInfoBox()
        {
            HelpInfoBoxVisible = !HelpInfoBoxVisible;
        }

        [RelayCommand]
        private async Task ClearSelection()
        {
            SelectedExpenseType = null;
            List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
            ExpenseTypeList = x.ToObservableCollection();
        }
    }
}
