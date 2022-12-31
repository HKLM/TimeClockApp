using Microsoft.EntityFrameworkCore;

using TimeClockApp.Models;

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
        //public int GetProfitRate()
        //{
        //    if (App.ProfitRate == 0)
        //        App.ProfitRate = GetConfigInt(7, 0);

        //    return App.ProfitRate;
        //}



        #region GETPROJECTDETAILS
        public (double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours) GetProjectDetails(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            double TotalExpenses = GetTotalExpensesPerProject(projectId, phaseId, filterDateFrom, filterDateTo);
            double TotalIncome = GetTotalIncomePerProject(projectId, phaseId, filterDateFrom, filterDateTo);
            double TotalWages = GetTotalWagesPerProject(projectId, phaseId, filterDateFrom, filterDateTo);
            double TotalHours = GetTotalManHoursPerProject(projectId, phaseId, filterDateFrom, filterDateTo);
            return (TotalExpenses, TotalIncome, TotalWages, TotalHours);
        }
        public (double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours) GetProjectDetails(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            double TotalExpenses = GetTotalExpensesPerProject(projectId, filterDateFrom, filterDateTo);
            double TotalIncome = GetTotalIncomePerProject(projectId, filterDateFrom, filterDateTo);
            double TotalWages = GetTotalWagesPerProject(projectId, filterDateFrom, filterDateTo);
            double TotalHours = GetTotalManHoursPerProject(projectId, filterDateFrom, filterDateTo);
            return (TotalExpenses, TotalIncome, TotalWages, TotalHours);
        }
        public (double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours) GetProjectDetails(int projectId, int phaseId)
        {
            double TotalExpenses = GetTotalExpensesPerProject(projectId, phaseId);
            double TotalIncome = GetTotalIncomePerProject(projectId, phaseId);
            double TotalWages = GetTotalWagesPerProject(projectId, phaseId);
            double TotalHours = GetTotalManHoursPerProject(projectId, phaseId);
            return (TotalExpenses, TotalIncome, TotalWages, TotalHours);
        }
        public (double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours) GetProjectDetails(int projectId)
        {
            double TotalExpenses = GetTotalExpensesPerProject(projectId);
            double TotalIncome = GetTotalIncomePerProject(projectId);
            double TotalWages = GetTotalWagesPerProject(projectId);
            double TotalHours = GetTotalManHoursPerProject(projectId);
            return (TotalExpenses, TotalIncome, TotalWages, TotalHours);
        }
        #endregion

        #region GETPROJECTDETAILS ASYNC
        public async Task<(double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours)> GetProjectDetailsAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            Task<double> TotalExpenses = GetTotalExpensesPerProjectAsync(projectId, phaseId, filterDateFrom, filterDateTo);
            Task<double> TotalIncome = GetTotalIncomePerProjectAsync(projectId, phaseId, filterDateFrom, filterDateTo);
            Task<double> TotalWages = GetTotalWagesPerProjectAsync(projectId, phaseId, filterDateFrom, filterDateTo);
            Task<double> TotalHours = GetTotalManHoursPerProjectAsync(projectId, phaseId, filterDateFrom, filterDateTo);
            return (await TotalExpenses, await TotalIncome, await TotalWages, await TotalHours);
        }
        public async Task<(double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours)> GetProjectDetailsAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            Task<double> TotalExpenses = GetTotalExpensesPerProjectAsync(projectId, filterDateFrom, filterDateTo);
            Task<double> TotalIncome = GetTotalIncomePerProjectAsync(projectId, filterDateFrom, filterDateTo);
            Task<double> TotalWages = GetTotalWagesPerProjectAsync(projectId, filterDateFrom, filterDateTo);
            Task<double> TotalHours = GetTotalManHoursPerProjectAsync(projectId, filterDateFrom, filterDateTo);
            return (await TotalExpenses, await TotalIncome, await TotalWages, await TotalHours);
        }
        public async Task<(double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours)> GetProjectDetailsAsync(int projectId, int phaseId)
        {
            Task<double> TotalExpenses = GetTotalExpensesPerProjectAsync(projectId, phaseId);
            Task<double> TotalIncome = GetTotalIncomePerProjectAsync(projectId, phaseId);
            Task<double> TotalWages = GetTotalWagesPerProjectAsync(projectId, phaseId);
            Task<double> TotalHours = GetTotalManHoursPerProjectAsync(projectId, phaseId);
            return (await TotalExpenses, await TotalIncome, await TotalWages, await TotalHours);
        }
        public async Task<(double TotalExpenses, double TotalIncome, double TotalWages, double TotalHours)> GetProjectDetailsAsync(int projectId)
        {
            Task<double> TotalExpenses = GetTotalExpensesPerProjectAsync(projectId);
            Task<double> TotalIncome = GetTotalIncomePerProjectAsync(projectId);
            Task<double> TotalWages = GetTotalWagesPerProjectAsync(projectId);
            Task<double> TotalHours = GetTotalManHoursPerProjectAsync(projectId);
            return (await TotalExpenses, await TotalIncome, await TotalWages, await TotalHours);
        }
        #endregion


        #region TOTAL EXPENSES
        public double GetTotalExpensesPerProject(int projectId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }

        public double GetTotalExpensesPerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }

        public double GetTotalExpensesPerProject(int projectId, int phaseId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }
        public double GetTotalExpensesPerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }
        #endregion

        #region TOTAL INCOME
        public double GetTotalIncomePerProject(int projectId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }

        public double GetTotalIncomePerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }

        public double GetTotalIncomePerProject(int projectId, int phaseId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Include(item => item.Project)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
        }
        public double GetTotalIncomePerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return t.Any() ? t.Sum(o => o.Amount) : 0;
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
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return t.Any() ? t.Sum(o => o) : 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public double GetTotalWagesPerProject(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return t.Any() ? t.Sum(o => o) : 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public double GetTotalWagesPerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return t.Any() ? t.Sum(o => o) : 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public double GetTotalWagesPerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return t.Any() ? t.Sum(o => o) : 0;
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
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return t.Any() ? t.Sum(o => o) : 0;
        }
        public double GetTotalManHoursPerProject(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return t.Any() ? t.Sum(o => o) : 0;
        }
        public double GetTotalManHoursPerProject(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return t.Any() ? t.Sum(o => o) : 0;
        }
        public double GetTotalManHoursPerProject(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return t.Any() ? t.Sum(o => o) : 0;
        }
        #endregion

        #region ASYNC TOTAL EXPENSES
        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }

        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }

        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId, int phaseId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }
        public async Task<double> GetTotalExpensesPerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory != ExpenseType.Deleted
                    && item.Catagory > ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }
        #endregion

        #region ASYNC TOTAL INCOME
        public async Task<double> GetTotalIncomePerProjectAsync(int projectId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }

        public async Task<double> GetTotalIncomePerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }

        public async Task<double> GetTotalIncomePerProjectAsync(int projectId, int phaseId)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= item.Project.ProjectDate));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
        }
        public async Task<double> GetTotalIncomePerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            IQueryable<Expense> t = Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.IsRecent
                    && item.Catagory == ExpenseType.Income
                    && (item.ExpenseDate >= filterDateFrom
                    && item.ExpenseDate <= filterDateTo));

            return await t.AnyAsync() ? await t.SumAsync(o => o.Amount) : 0;
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
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }

        /// <inheritdoc cref="GetTotalWagesPerProject(int)"/>
        public async Task<double> GetTotalWagesPerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalWages);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }
        #endregion

        #region ASYNC TOTAL MAN HOURS
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId, int phaseId)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                //.Include(item => item.Project)
                .Select(item => item.Wages.TotalHours);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }
        public async Task<double> GetTotalManHoursPerProjectAsync(int projectId, int phaseId, DateOnly filterDateFrom, DateOnly filterDateTo)
        {
            ShiftStatus s = ShiftStatus.ClockedOut;
            ShiftStatus p = ShiftStatus.Paid;
            IQueryable<double> t = Context.TimeCard
                .AsNoTracking()
                .Include(item => item.Wages)
                .Where(item => item.ProjectId == projectId
                    && item.PhaseId == phaseId
                    && item.TimeCard_Date >= filterDateFrom
                    && item.TimeCard_Date <= filterDateTo
                    && (item.TimeCard_Status == s
                    || item.TimeCard_Status == p)
                    && item.WagesId.HasValue)
                .Select(item => item.Wages.TotalHours);

            return await t.AnyAsync() ? await t.SumAsync(o => o) : 0;
        }
        #endregion
    }
}
