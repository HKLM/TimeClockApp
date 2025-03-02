using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Services
{
    public class EditExpenseTypeService : TimeCardDataStore
    {
        public Task<List<ExpenseType>> GetExpenseTypeListAsync() =>
                Context.ExpenseType
                    .Where(item => item.ExpenseTypeId > 1)
                    .OrderBy(item => item.CategoryName)
                    .ToListAsync();

        /// <summary>
        /// Adds new CategoryName of ExpenseType type
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns>
        /// 0=Other error
        /// 1=successful
        /// 2=Error, duplicate name
        /// </returns>
        public async Task<int> AddExpenseTypeAsync(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                return 0;

            if (await Context.ExpenseType.AsNoTracking().Where(x => x.CategoryName == categoryName).AnyAsync())
            {
                return 2;
            }
            else
            {
                ExpenseType et = new()
                {
                    CategoryName = categoryName
                };
                Context.Add<ExpenseType>(et);
                return (await Context.SaveChangesAsync() > 0) ? 1 : 0;
            }
        }

        public async Task<bool> UpdateExpenseTypeAsync(int expenseTypeId, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName) || expenseTypeId == 0)
                return false;

            ExpenseType origExpense = Context.ExpenseType.Find(expenseTypeId);
            if (origExpense != null)
            {
                origExpense.CategoryName = categoryName;
                Context.Update<ExpenseType>(origExpense);
                return (await Context.SaveChangesAsync() > 0);
            }
            return false;
        }

        public async Task<bool> DeleteExpenseTypeAsync(int expenseTypeId)
        {
            if (expenseTypeId == 0)
                return false;

            ExpenseType origExpense = Context.ExpenseType.Find(expenseTypeId);
            if (origExpense != null)
            {
                if (await Context.ExpenseType.CountAsync() > 2)
                {
                    Context.Remove<ExpenseType>(origExpense);
                    return (await Context.SaveChangesAsync() > 0);
                }
            }
            return false;
        }
    }
}
