using CommunityToolkit.Maui.Core.Extensions;

using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Services
{
    public partial class ExpenseService : TimeCardDataStore
    {
        public Expense GetExpense(int expenseId)
        {
            return Context.Expense
                .Include(item => item.Project)
                .First(item => item.ExpenseId == expenseId);
        }

        public ObservableCollection<Expense> GetAllExpenses(int projectId, bool showRecent = true)
        {
            return Context.Expense
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.Category != ExpenseType.Deleted
                    && item.IsRecent == showRecent)
                .OrderByDescending(e => e.ExpenseDate)
                .ToObservableCollection();
        }

        public bool AddNewExpense(int projectId, int phaseId, double amount, string memo, string projectName, string phaseTitle, ExpenseType category = ExpenseType.Materials)
        {
            if (projectId == 0) return false;
            //make expenses a negative number
            if (category != ExpenseType.Income)
                amount *= (-1);

            Expense exp = new(projectId, phaseId, amount, DateOnly.FromDateTime(DateTime.Now), projectName, phaseTitle, memo, category);

            Context.Add<Expense>(exp);
            return (Context.SaveChanges() > 0);
        }

        public bool UpdateExpense(Expense newExpense)
        {
            if (newExpense == null || newExpense.ExpenseId == 0)
                return false;

            Expense origExpense = Context.Expense.Find(newExpense.ExpenseId);
            if (origExpense != null)
            {
                origExpense.Amount = newExpense.Amount;
                origExpense.Category = newExpense.Category;
                origExpense.Memo = newExpense.Memo;
                origExpense.ExpenseDate = newExpense.ExpenseDate;
                origExpense.ProjectId = newExpense.ProjectId;
                origExpense.PhaseId = newExpense.PhaseId;
                origExpense.IsRecent = newExpense.IsRecent;
                origExpense.ExpenseProject = newExpense.ExpenseProject;
                origExpense.ExpensePhase = newExpense.ExpensePhase;
                Context.Update<Expense>(origExpense);
                return (Context.SaveChanges() > 0);
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
                Context.Remove<Expense>(origExpense);
                return (await Context.SaveChangesAsync() > 0);
            }
            return false;
        }

        public bool ArchiveExpense(ObservableCollection<Expense> expenseList)
        {
            if (expenseList == null || expenseList.Count == 0)
                return false;

            try
            {
                foreach (Expense item in expenseList)
                {
                    item.IsRecent = false;
                    Context.Update<Expense>(item);
                }
                return (Context.SaveChanges() > 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                ShowPopupError(ex.Message + "\n" + ex.InnerException, "Exception");
            }
            return false;
        }
    }
}
