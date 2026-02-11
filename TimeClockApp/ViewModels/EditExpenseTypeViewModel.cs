#nullable enable

namespace TimeClockApp.ViewModels
{
    public partial class EditExpenseTypeViewModel : BaseViewModel, IQueryAttributable
    {
        protected readonly EditExpenseTypeService dataService = new ();

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                if (Int32.TryParse(query["id"].ToString(), out int i))
                { ExpenseId = i; }
            }
        }

        [ObservableProperty]
        public partial int ExpenseId { get; set; } = 0;
        partial void OnExpenseIdChanged(int value)
        {
            if (value > 0)
            {
                SelectedExpenseType = dataService.GetExpenseType(value);
            }
        }

        [ObservableProperty]
        public partial ObservableCollection<ExpenseType> ExpenseTypeList { get; set; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableDeleteButton))]
        public partial ExpenseType? SelectedExpenseType { get; set; } = new();
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
        public partial string NewCategoryName { get; set; } = string.Empty;

        public bool EnableAddButton => !string.IsNullOrEmpty(NewCategoryName);
        public bool EnableDeleteButton => SelectedExpenseType != null;

        public async Task OnAppearing()
        {
            List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
            ExpenseTypeList = x.ToObservableCollection();
        }

        [RelayCommand]
        private async Task AddExpenseTypeAsync()
        {
            if (string.IsNullOrEmpty(NewCategoryName))
            {
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "ExpenseType must have a name.").ConfigureAwait(false);
                return;
            }

            string newName = NewCategoryName.Trim();
            int i = await dataService.AddExpenseTypeAsync(newName);
            if (i == 1)
            {
                SelectedExpenseType = null;
                List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                ExpenseTypeList = x.ToObservableCollection();
                NewCategoryName = string.Empty;
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "Saved").ConfigureAwait(false);
            }
            else if (i == 2)
            {
                await App.AlertSvc!.ShowAlertAsync("DUPLICATE", newName + " already exists in database. Failed to add new ExpenseType.").ConfigureAwait(false);
            }
            else
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "Failed to add new ExpenseType").ConfigureAwait(false);
        }

        [RelayCommand]
        private async Task EditExpenseTypeAsync()
        {
            if (SelectedExpenseType == null || string.IsNullOrEmpty(NewCategoryName))
                return;
            if (SelectedExpenseType.ExpenseTypeId < 6)
            {
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "This ExpenseType is required and can not be edited.").ConfigureAwait(false);
                return;
            }

            try
            {
                string newName = NewCategoryName.Trim();
                if (await dataService.UpdateExpenseTypeAsync(SelectedExpenseType.ExpenseTypeId, newName))
                {
                    SelectedExpenseType = null;
                    List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                    ExpenseTypeList = x.ToObservableCollection();

                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Saved").ConfigureAwait(false);
                }
                else
                    await App.AlertSvc!.ShowAlertAsync("NOTICE", "Failed to save ExpenseType").ConfigureAwait(false);
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DelExpenseTypeAsync()
        {
            if (SelectedExpenseType == null)
                return;

            if (SelectedExpenseType.ExpenseTypeId < 6)
            {
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "This ExpenseType is required and can not be deleted.").ConfigureAwait(false);
                return;
            }

            try
            {
                Log.WriteLine("User is attempting to delete a ExpenseType record");
                if (await App.AlertSvc!.ShowConfirmationAsync("CONFIRMATION", "Are you sure you want to Delete this ExpenseType?"))
                {
                    if (await dataService.DeleteExpenseTypeAsync(SelectedExpenseType.ExpenseTypeId))
                    {
                        SelectedExpenseType = null;
                        List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                        ExpenseTypeList = x.ToObservableCollection();

                        await App.AlertSvc!.ShowAlertAsync("NOTICE", "Deleted").ConfigureAwait(false);
                        //await Shell.Current.GoToAsync("..");
                    }
                    else
                        await App.AlertSvc!.ShowAlertAsync("NOTICE", "Failed to delete ExpenseType").ConfigureAwait(false);
                }
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
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
