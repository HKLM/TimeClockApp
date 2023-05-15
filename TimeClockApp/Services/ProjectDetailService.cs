//using Meziantou.Framework;
using Microsoft.EntityFrameworkCore;

using TimeClockApp.Utilities;

namespace TimeClockApp.Services
{
    public partial class ProjectDetailService : ExpenseService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>WorkCompRate, EmployerPayroll, OverHead</returns>
        public (double WorkCompRate, double EmployerPayroll, int OverHead) GetTaxRates() => (GetCurrentWorkCompRate(), GetEstimatedEmployerPayrollTaxRate(), 0);

        public double GetCurrentWorkCompRate()
        {
            if (App.CurrentWorkCompRate == 0)
                App.CurrentWorkCompRate = (double)GetConfigInt(5, 0);

            return App.CurrentWorkCompRate;
        }
        public double GetEstimatedEmployerPayrollTaxRate()
        {
            if (App.EstimatedEmployerPayrollTaxRate == 0)
            {
                string s = GetConfigString(6);
                if (double.TryParse(s, out double taxrate))
                    App.EstimatedEmployerPayrollTaxRate = taxrate;
            }

            return App.EstimatedEmployerPayrollTaxRate;
        }
        public int GetOverHeadRate()
        {
            if (App.OverHeadRate == 0)
                App.OverHeadRate = GetConfigInt(8, 0);

            return App.OverHeadRate;
        }


        #region GETPROJECTDETAILS
        public (double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours) GetProjectDetails(int projectId, int? phaseId = null, DateOnly? filterDateFrom = null, DateOnly? filterDateTo = null)
        {
            double TotalExpenses;
            double TotalIncome;
            double TotalWages;
            double TotalHours;

            if (filterDateFrom.HasValue && filterDateTo.HasValue && phaseId.HasValue)
            {
                TotalExpenses = GetTotalExpensesPerProject(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
                TotalIncome = GetTotalIncomePerProject(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
                TotalWages = GetTotalWagesPerProject(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
                TotalHours = GetTotalManHoursPerProject(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
            }
            else if (filterDateFrom.HasValue && filterDateTo.HasValue)
            {
                TotalExpenses = GetTotalExpensesPerProject(projectId, filterDateFrom.Value, filterDateTo.Value);
                TotalIncome = GetTotalIncomePerProject(projectId, filterDateFrom.Value, filterDateTo.Value);
                TotalWages = GetTotalWagesPerProject(projectId, filterDateFrom.Value, filterDateTo.Value);
                TotalHours = GetTotalManHoursPerProject(projectId, filterDateFrom.Value, filterDateTo.Value);
            }
            else if (phaseId.HasValue)
            {
                TotalExpenses = GetTotalExpensesPerProject(projectId, phaseId.Value);
                TotalIncome = GetTotalIncomePerProject(projectId, phaseId.Value);
                TotalWages = GetTotalWagesPerProject(projectId, phaseId.Value);
                TotalHours = GetTotalManHoursPerProject(projectId, phaseId.Value);
            }
            else
            {
                TotalExpenses = GetTotalExpensesPerProject(projectId);
                TotalIncome = GetTotalIncomePerProject(projectId);
                TotalWages = GetTotalWagesPerProject(projectId);
                TotalHours = GetTotalManHoursPerProject(projectId);
            }
            return (TotalExpenses, TotalIncome, TotalWages, TotalHours);
        }
        #endregion

        #region GETPROJECTDETAILS ASYNC
        public async Task<(double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours)> GetProjectDetailsAsync(int projectId, int? phaseId = null, DateOnly? filterDateFrom = null, DateOnly? filterDateTo = null)
        {
            Task<double> TotalExpenses;
            Task<double> TotalIncome;
            Task<double> TotalWages;
            Task<double> TotalHours;

            if (filterDateFrom.HasValue && filterDateTo.HasValue && phaseId.HasValue)
            {
                TotalExpenses = GetTotalExpensesPerProjectAsync(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
                TotalIncome = GetTotalIncomePerProjectAsync(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
                TotalWages = GetTotalWagesPerProjectAsync(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
                TotalHours = GetTotalManHoursPerProjectAsync(projectId, phaseId.Value, filterDateFrom.Value, filterDateTo.Value);
            }
            else if (filterDateFrom.HasValue && filterDateTo.HasValue)
            {
                TotalExpenses = GetTotalExpensesPerProjectAsync(projectId, filterDateFrom.Value, filterDateTo.Value);
                TotalIncome = GetTotalIncomePerProjectAsync(projectId, filterDateFrom.Value, filterDateTo.Value);
                TotalWages = GetTotalWagesPerProjectAsync(projectId, filterDateFrom.Value, filterDateTo.Value);
                TotalHours = GetTotalManHoursPerProjectAsync(projectId, filterDateFrom.Value, filterDateTo.Value);
            }
            else if (phaseId.HasValue)
            {
                TotalExpenses = GetTotalExpensesPerProjectAsync(projectId, phaseId.Value);
                TotalIncome = GetTotalIncomePerProjectAsync(projectId, phaseId.Value);
                TotalWages = GetTotalWagesPerProjectAsync(projectId, phaseId.Value);
                TotalHours = GetTotalManHoursPerProjectAsync(projectId, phaseId.Value);
            }
            else
            {
                TotalExpenses = GetTotalExpensesPerProjectAsync(projectId);
                TotalIncome = GetTotalIncomePerProjectAsync(projectId);
                TotalWages = GetTotalWagesPerProjectAsync(projectId);
                TotalHours = GetTotalManHoursPerProjectAsync(projectId);
            }
            var (t1Result, t2Result, t3Result, t4Result) = await (TotalExpenses, TotalIncome, TotalWages, TotalHours);
            return (t1Result, t2Result, t3Result, t4Result);
        }
        #endregion


        #region TOTAL EXPENSES
        public double GetTotalExpensesPerProject(int projectId)
        {
            return Context.Expense
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        public double GetTotalExpensesPerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        public double GetTotalExpensesPerProject(int projectId, int phaseId)
        {
            return Context.Expense
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        public double GetTotalExpensesPerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        #endregion

        #region TOTAL INCOME
        public double GetTotalIncomePerProject(int projectId)
        {
            return Context.Expense.TagWithCallSite().TagWith("TotalIncomePerProject-1")
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        public double GetTotalIncomePerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return Context.Expense.TagWithCallSite().TagWith("TotalIncomePerProject-2")
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        public double GetTotalIncomePerProject(int projectId, int phaseId)
        {
            return Context.Expense.TagWithCallSite().TagWith("TotalIncomePerProject-2")
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        public double GetTotalIncomePerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return Context.Expense.TagWithCallSite().TagWith("TotalIncomePerProject-4")
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .Sum(item => (double?)item.Amount) ?? 0;
        }
        #endregion

        //WAGES
        #region TOTAL WAGES
        /// <summary>
        /// Gets the combined total wages for the given project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public double GetTotalWagesPerProject(int projectId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalWages) ?? 0;
        }
        public double GetTotalWagesPerProject(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalWages) ?? 0;
        }
        public double GetTotalWagesPerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalWages) ?? 0;
        }
        public double GetTotalWagesPerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalWages) ?? 0;
        }
        #endregion

        #region TOTAL MAN HOURS
        /// <summary>
        /// Gets the combined total man hours for the given project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public double GetTotalManHoursPerProject(int projectId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        public double GetTotalManHoursPerProject(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        public double GetTotalManHoursPerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        public double GetTotalManHoursPerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Sum(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        #endregion

        #region ASYNC TOTAL EXPENSES
        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId)
        {
            return await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                 .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                  .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId, int phaseId)
        {
            return await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
               .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category != ExpenseType.Deleted
                    && item.Category > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        #endregion

        #region ASYNC TOTAL INCOME
        public async Task<double> GetTotalIncomePerProjectAsync(int projectId)
        {
            return await Context.Expense.TagWithCallSite().TagWith("async_TotalIncomePerProject-1")
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        public async Task<double> GetTotalIncomePerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return await Context.Expense.TagWithCallSite().TagWith("async_TotalIncomePerProject-2")
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        public async Task<double> GetTotalIncomePerProjectAsync(int projectId, int phaseId)
        {
            return await Context.Expense.TagWithCallSite().TagWith("async_TotalIncomePerProject-3")
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate))
                .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        public async Task<double> GetTotalIncomePerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            return await Context.Expense.TagWithCallSite().TagWith("async_TotalIncomePerProject-4")
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Category == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo))
                .SumAsync(item => (double?)item.Amount) ?? 0;
        }
        #endregion

        //ASYNC WAGES
        #region ASYNC TOTAL WAGES PER PROJECT
        /// <summary>
        /// Gets the combined total wages for the given project
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalWages) ?? 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalWages) ?? 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalWages) ?? 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalWages) ?? 0;
        }
        #endregion

        #region ASYNC TOTAL MAN HOURS
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            return await Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .SumAsync(item => (double?)item.Wages.TotalHours) ?? 0;
        }
        #endregion
    }
}
