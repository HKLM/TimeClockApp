using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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
            string message = i switch
            {
                1 => "Saved",
                2 => newName + " already exists in database. Failed to add new ExpenseType.",
                _ => "Failed to add new ExpenseType"
            };
            if (i == 1)
            {
                SelectedExpenseType = null;
                List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                ExpenseTypeList = x.ToObservableCollection();
                NewCategoryName = string.Empty;
            }
            //Toast
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var toast = Toast.Make(message, ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
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
                bool success = await dataService.UpdateExpenseTypeAsync(SelectedExpenseType.ExpenseTypeId, newName);
                string message = success ? "Saved" : "Failed to save ExpenseType";
                //Toast
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                var toast = Toast.Make(message, ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);
                if (success)
                {
                    SelectedExpenseType = null;
                    List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                    ExpenseTypeList = x.ToObservableCollection();
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
                    bool success = await dataService.DeleteExpenseTypeAsync(SelectedExpenseType.ExpenseTypeId);
                    string message = success ? "Deleted" : "Failed to delete ExpenseType";
                    //Toast
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    var toast = Toast.Make(message, ToastDuration.Short, 14);
                    await toast.Show(cancellationTokenSource.Token);
                    if (success)
                    {
                        SelectedExpenseType = null;
                        List<ExpenseType> x = await dataService.GetExpenseTypeListAsync();
                        ExpenseTypeList = x.ToObservableCollection();
                    }
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
