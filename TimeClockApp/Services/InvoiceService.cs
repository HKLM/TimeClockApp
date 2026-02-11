using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace TimeClockApp.Services
{
    public class InvoiceService : ReportPageService
    {
        /// <summary>
        /// Gets the configured profit rate value from the database
        /// </summary>
        //public double GetProfitRate() => GetRate(7);
        public Task<double> GetProfitRateAsync() => GetRateAsync(7);
        /// <summary>
        /// Gets the configured overhead rate value from the database
        /// </summary>
        //public double GetOverheadRate() => GetRate(8);
        public Task<double> GetOverheadRateAsync() => GetRateAsync(8);

        public async Task<List<TimeSheet>> RunInvoiceReportAsync(bool usePhaseFilter, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            ArgumentNullException.ThrowIfNull(project);

            List<TimeSheet> t = [];

            start ??= DateOnly.FromDateTime(GetAppFirstRunDate());
            end ??= DateOnly.FromDateTime(DateTime.Now);
            IQueryable<Employee> e = QGetEmployeeListForProject(project.ProjectId, start.Value, end.Value);

            foreach (var emp in e)
            {
                TimeSheet sheet = new TimeSheet(emp.EmployeeId, start.Value, end.Value, emp.Employee_Name);
                sheet.TimeCards = await TimeCardsForPayPeriod(true, usePhaseFilter, true, emp, project, phase, start, end)
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (sheet.TimeCards.Count <= 0)
                {
                    t.Add(sheet);
                    continue;
                }

                double pay = 0;
                var groupedByDate = sheet.TimeCards.GroupBy(x => x.TimeCard_Date).ToList();

                foreach (var g in groupedByDate)
                {
                    var validCards = g.Where(x => x.TimeCard_Status is ShiftStatus.ClockedOut or ShiftStatus.Paid).ToList();
                    foreach (var item in validCards)
                    {
                        if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                            sheet.UnpaidTimeCards.Add(item);
                        else if (item.TimeCard_Status == ShiftStatus.Paid)
                            sheet.PaidTimeCards.Add(item);
                    }

                    pay = validCards[0].TimeCard_EmployeePayRate;

                    var dayHours = new TimeCardDayTotal(validCards.Sum(x => x.TimeCard_WorkHours));
                    sheet.TotalWorkHours += dayHours.TotalWorkHours;
                    sheet.RegTotalHours += dayHours.RegTotalHours;
                    sheet.TotalOTHours += dayHours.TotalOTHours;
                    sheet.TotalOT2Hours += dayHours.TotalOT2Hours;

                    double unpaidHours = validCards.Where(x => x.TimeCard_Status == ShiftStatus.ClockedOut).Sum(x => x.TimeCard_WorkHours);
                    if (unpaidHours > 0)
                    {
                        var unpaidDayHours = new TimeCardDayTotal(unpaidHours);
                        sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                        sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                        sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                        sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
                    }
                }

                sheet.RegTotalPay = sheet.RegTotalHours * pay;
                sheet.TotalOTPay = sheet.TotalOTHours * (pay * 1.5);
                sheet.TotalOT2Pay = sheet.TotalOT2Hours * (pay * 2);
                sheet.TotalGrossPay = sheet.RegTotalPay + sheet.TotalOTPay + sheet.TotalOT2Pay;
                t.Add(sheet);
            }
            return t;
        }

        public IQueryable<Employee> QGetEmployeeListForProject(int projectId, DateOnly start, DateOnly end)
        {
            return Context.Employee
                .Include(e => e.TimeCards
                        .Where(t => t.ProjectId == projectId
                            && t.TimeCard_Date >= start
                            && t.TimeCard_Date <= end
                            && (t.TimeCard_Status == ShiftStatus.ClockedOut
                            || t.TimeCard_Status == ShiftStatus.Paid)))
                .Where(e => e.Employee_Employed != EmploymentStatus.Deleted)
                .AsQueryable();
        }

        public async Task<double> GetProjectExpensesAmountAsync(int projectId, DateOnly start, DateOnly end, bool allItems)
        {
            double e;
            int i = allItems ? 1 : 4;
            e = await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId && item.ExpenseTypeId > i && item.ExpenseDate >= start && item.ExpenseDate <= end)
                .SumAsync(item => item.Amount)
                .ConfigureAwait(false);

            return Math.Abs(e);
        }

        public Task<List<Expense>> GetProjectExpensesListAsync(int projectId, DateOnly start, DateOnly end, bool allItems)
        {
            int i = allItems ? 1 : 4;
            return Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId && item.ExpenseTypeId > i && item.ExpenseDate >= start && item.ExpenseDate <= end)
                .ToListAsync();
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions))]
        protected IQueryable<TimeCard> TimeCardsForPayPeriod(bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, Employee employee, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            IQueryable<TimeCard> q = Context.TimeCard
                .AsNoTrackingWithIdentityResolution()
                .AsQueryable();

            q = q.Where(item => item.EmployeeId == employee.EmployeeId
                    && (item.TimeCard_Status == ShiftStatus.ClockedOut
                    || item.TimeCard_Status == ShiftStatus.Paid));

            if (useProjectFilter && project != null)
            {
                q = q.Where(item => item.ProjectId == project.ProjectId);
            }
            if (usePhaseFilter && phase != null)
            {
                q = q.Where(item => item.PhaseId == phase.PhaseId);
            }
            if (useDateFilter && start.HasValue && end.HasValue)
            {
                q = q.Where(item =>
                        item.TimeCard_Date >= start!.Value
                     && item.TimeCard_Date <= end!.Value);
            }

            return q.OrderBy(item => item.TimeCard_Date);
        }
    }
}
