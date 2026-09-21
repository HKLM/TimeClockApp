using TimeClockApp.Shared;
using TimeClockApp.Shared.Interfaces;

namespace TimeClockApp.ViewModels
{
    public partial class EditExpensePageViewModel(ExpenseService _dataService, IProjectListService projectListService) : BaseViewModel, IQueryAttributable
    {
        protected readonly ExpenseService dataService = _dataService;
		private readonly IProjectListService _projectListService = projectListService;
		public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
			if (query.ContainsKey("id") && Int32.TryParse(query["id"].ToString(), out int i))
			{ ExpenseId = i; }
		}

        [ObservableProperty]
        public partial int ExpenseId { get; set; } = 0;
        partial void OnExpenseIdChanged(int value)
        {
            Refresh();
        }

        [ObservableProperty]
        public partial Expense ExpenseItem { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Project> ProjectList { get; set; } = [];
        [ObservableProperty]
        public partial Project SelectedProject { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<Phase> PhaseList { get; set; } = [];
        [ObservableProperty]
        public partial Phase SelectedPhase { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<ExpenseType> ExpenseTypeList { get; set; } = [];
        [ObservableProperty]
        public partial ExpenseType SelectedExpenseType { get; set; } = new();

        [ObservableProperty]
        public partial DateOnly ExpenseDate { get; set; }
#region "DatePicker Min/Max Bindings"
        public DateTime PickerMinDate { get; set; }
        private readonly DateTime pickerMaxDate = DateTime.Now;
        public DateTime PickerMaxDate { get => pickerMaxDate; }
#endregion
        [ObservableProperty]
        public partial Double Amount { get; set; }
        [ObservableProperty]
        public partial string Memo { get; set; }
        [ObservableProperty]
        public partial bool IsRefreshingList { get; set; }

        public void OnAppearing()
        {
            PickerMinDate = dataService.GetAppFirstRunDate();
            ExpenseDate = DateOnly.FromDateTime(DateTime.Now);
            if (Preferences.ContainsKey("IsAdmin", PrefSetting.PrefKey))
            {
                IsAdmin = Preferences.Get("IsAdmin", IsAdmin, PrefSetting.PrefKey);
            }
            else
            {
                IsAdmin = IntToBool(dataService.GetConfigInt(9, 0));
                Preferences.Set("IsAdmin", IsAdmin, PrefSetting.PrefKey);
            }

            Refresh();
        }

        private void Refresh()
        {
            RefreshProjectPhases();
            if (ExpenseId != 0)
            {
                ExpenseItem = dataService.GetExpense(ExpenseId);

                Amount = ExpenseItem.Amount;
                Memo = ExpenseItem.Memo;
                ExpenseDate = ExpenseItem.ExpenseDate;
                SelectedProject = dataService.GetProject(ExpenseItem.ProjectId);
                SelectedPhase = dataService.GetPhase(ExpenseItem.PhaseId);
                SelectedExpenseType = dataService.GetExpenseType(ExpenseItem.ExpenseTypeId);
            }
        }

        private void RefreshProjectPhases()
        {
			MainThread.BeginInvokeOnMainThread(async () => { await RefreshProjectPhaseListAsync(); });

            ExpenseTypeList ??= [];
            List<ExpenseType> x = dataService.GetExpenseTypeList();
            ExpenseTypeList = x.ToObservableCollection();
        }

		private async Task RefreshProjectPhaseListAsync()
		{
			//List<Project> p = await _projectListService.GetProjectsListAsync();
			//ProjectList = p.ToObservableCollection();
			ProjectList = dataService.GetAllProjectsList(IsAdmin);
			List<Phase> ph = await _projectListService.GetPhaseListAsync();
			PhaseList = ph.ToObservableCollection();
		}
		[RelayCommand]
        private async Task EditExpenseAsync()
        {
            if (ExpenseItem == null || ExpenseItem.ExpenseId == 0)
                return;
            if (string.IsNullOrEmpty(SelectedProject.Name))
            {
                await App.AlertSvc!.ShowAlertAsync("NOTICE", "This record can not be edited. This maybe due to the Project has been archived and only a Admin can edit this record.").ConfigureAwait(false);
                return;
            }

            try
            {
                string expenseNewMemo = Memo.Trim();
                ExpenseItem.Amount = Amount;
                ExpenseItem.Memo = expenseNewMemo;
                ExpenseItem.ExpenseDate = ExpenseDate;
                ExpenseItem.ProjectId = SelectedProject.ProjectId;
                ExpenseItem.PhaseId = SelectedPhase.PhaseId;
                ExpenseItem.ExpenseProject = SelectedProject.Name;
                ExpenseItem.ExpensePhase = SelectedPhase.PhaseTitle;
                ExpenseItem.ExpenseTypeId = SelectedExpenseType.ExpenseTypeId;
                ExpenseItem.ExpenseTypeCategoryName = SelectedExpenseType.CategoryName;

                bool success = dataService.UpdateExpense(ExpenseItem);
                string message = success ? "Saved" : "Failed to save Expense";
				await AppShell.DisplayToastAsync(message);
				if (success) 
                    Refresh();
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }

        [RelayCommand]
        private async Task DelExpenseAsync()
        {
            if (ExpenseItem == null || ExpenseItem.ExpenseId == 0)
                return;

            try
            {
                Log.WriteLine("User is attempting to delete a Expense record");
                if (await App.AlertSvc!.ShowConfirmationAsync("CONFIRMATION", "Are you sure you want to Delete this expense?"))
                {
                    bool success = await dataService.DeleteExpense(ExpenseItem);
                    string message = success ? "Deleted" : "Failed to delete Expense";
					await AppShell.DisplayToastAsync(message);
					if (success)
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                }
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
    }
}
