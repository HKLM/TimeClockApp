using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Services
{
    public class ExpenseService : TimeCardDataStore
    {
        public Expense GetExpense(int expenseId) => Context.Expense.Find(expenseId);

        public List<ExpenseType> GetExpenseTypeList() =>
            [.. Context.ExpenseType
                .Where(item => item.ExpenseTypeId > 1)
                .OrderBy(item => item.CategoryName)];

        public async Task<List<ExpenseType>> GetExpenseTypeListAsync() =>
            await Context.ExpenseType
                .Where(item => item.ExpenseTypeId > 1)
                .OrderBy(item => item.CategoryName)
                .ToListAsync();

        public async Task<List<Expense>> GetRecentExpensesListAsync(int projectId, bool showRecent = true, int numOfResults = 20) =>
            await Context.Expense
                .Where(item => item.ProjectId == projectId
                    && item.ExpenseTypeId > 1
                    && item.IsRecent == showRecent)
                .OrderByDescending(e => e.ExpenseDate)
                .Take(numOfResults)
                .ToListAsync();

        public async Task<List<Expense>> GetAllExpensesListAsync(int numOfResults) =>
            await Context.Expense
                .Where(item => item.ExpenseTypeId > 1)
                .OrderByDescending(e => e.ExpenseDate)
                .Take(numOfResults)
                .ToListAsync();

        public bool UpdateExpense(Expense newExpense)
        {
            if (newExpense == null || newExpense.ExpenseId == 0)
                return false;

            Expense origExpense = Context.Expense.Find(newExpense.ExpenseId);
            if (origExpense != null)
            {
                //make expenses (not income) a negative number
                if (newExpense.ExpenseTypeId != 2 && newExpense.Amount > 0)
                    newExpense.Amount *= (-1);

                origExpense.Amount = newExpense.Amount;
                origExpense.ExpenseTypeId = newExpense.ExpenseTypeId;
                origExpense.Memo = newExpense.Memo;
                origExpense.ExpenseDate = newExpense.ExpenseDate;
                origExpense.ProjectId = newExpense.ProjectId;
                origExpense.PhaseId = newExpense.PhaseId;
                origExpense.IsRecent = newExpense.IsRecent;
                origExpense.ExpenseProject = newExpense.ExpenseProject;
                origExpense.ExpensePhase = newExpense.ExpensePhase;
                origExpense.ExpenseTypeCategoryName = newExpense.ExpenseTypeCategoryName;
                Context.Update<Expense>(origExpense);
                return Context.SaveChanges() > 0;
            }
            return false;
        }

        public async Task<bool> DeleteExpense(Expense delExpense)
        {
            if (delExpense == null || delExpense.ExpenseId == 0)
                return false;

            Expense origExpense = Context.Expense.Find(delExpense.ExpenseId);
            if (origExpense != null)
            {
                origExpense.ExpenseTypeId = 1;
                Context.Update<Expense>(origExpense);
                return await Context.SaveChangesAsync() > 0;
            }
            return false;
        }

        //public bool ArchiveExpense(ObservableCollection<Expense> expenseList)
        //{
        //    if (expenseList == null || expenseList.Count == 0)
        //        return false;

        //    try
        //    {
        //        foreach (Expense item in expenseList)
        //        {
        //            item.IsRecent = false;
        //            Context.Update<Expense>(item);
        //        }
        //        return Context.SaveChanges() > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLine(ex.Message + "\n" + ex.InnerException);
        //        ShowPopupError(ex.Message + "\n" + ex.InnerException, "Exception");
        //    }
        //    return false;
        //}
    }
}
