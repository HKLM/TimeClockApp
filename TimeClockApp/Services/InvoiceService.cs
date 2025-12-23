using Microsoft.EntityFrameworkCore;

#nullable enable

namespace TimeClockApp.Services
{
    public class InvoiceService : ReportPageService
    {
        /// <summary>
        /// Gets the configured profit rate value from the database
        /// </summary>
        public double GetProfitRate() => GetRate(7);
        /// <summary>
        /// Gets the configured overhead rate value from the database
        /// </summary>
        public double GetOverheadRate() => GetRate(8);

        public async Task<List<TimeSheet>> RunInvoiceReportAsync(bool usePhaseFilter, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            if (project == null)
            {
                Log.WriteLine("RunInvoiceReportAsync project is null, aborting");
                return new List<TimeSheet>();
            }

            List<TimeSheet> t = [];
            IEnumerable<Employee> e = [];

            start ??= DateOnly.FromDateTime(GetAppFirstRunDate());
            end ??= DateOnly.FromDateTime(DateTime.Now);
            //e = await GetEmployeeListForProjectAsync(project.ProjectId, (DateOnly)start, (DateOnly)end);
            var el = GetEmployeeListForProject(project.ProjectId, (DateOnly)start, (DateOnly)end);
            e = el.DistinctBy(x => x.EmployeeId).AsEnumerable();
            if (e.Any())
            {
                foreach (Employee emp in e)
                {
                    TimeSheet sheet = new TimeSheet(emp.EmployeeId, start.Value, end.Value, emp.Employee_Name);
                    sheet.TimeCards = await TimeCardsForPayPeriod(true, usePhaseFilter, true, emp, project, phase, start, end).ToListAsync().ConfigureAwait(false);

                    if (sheet.TimeCards.Count > 0)
                    {
                        double pay = 0;
                        IQueryable<IGrouping<DateOnly, TimeCard>> tg = sheet.TimeCards.GroupBy(x => x.TimeCard_Date).AsQueryable();
                        foreach (IGrouping<DateOnly, TimeCard> g in tg)
                        {
                            double x = 0;
                            foreach (TimeCard item in g)
                            {
                                if (item.TimeCard_Status == ShiftStatus.ClockedOut)
                                {
                                    sheet.UnpaidTimeCards.Add(item);
                                    x += item.TimeCard_WorkHours;
                                }
                                else if (item.TimeCard_Status == ShiftStatus.Paid)
                                {
                                    sheet.PaidTimeCards.Add(item);
                                    x += item.TimeCard_WorkHours;
                                }
                            }

                            //TODO fix
                            pay = g.Select(x => x.TimeCard_EmployeePayRate).First();

                            //dont include any cards ClockedIn. They wreak havoc on the calculations.
                            TimeCardDayTotal dayHours = new TimeCardDayTotal(g
                                .Where(x =>
                                x.TimeCard_Status == ShiftStatus.ClockedOut
                                || x.TimeCard_Status == ShiftStatus.Paid)
                                .Sum(x => x.TimeCard_WorkHours));

                            sheet.TotalWorkHours += dayHours.TotalWorkHours;
                            sheet.RegTotalHours += dayHours.RegTotalHours;
                            sheet.TotalOTHours += dayHours.TotalOTHours;
                            sheet.TotalOT2Hours += dayHours.TotalOT2Hours;

                            // Calculate unpaid hours
                            double d = g.Where(x =>
                                            x.TimeCard_Status == ShiftStatus.ClockedOut)
                                            .Sum(x => x.TimeCard_WorkHours);
                            if (d > 0)
                            {
                                TimeCardDayTotal unpaidDayHours = new(d);
                                sheet.UnPaidTotalWorkHours += unpaidDayHours.TotalWorkHours;
                                sheet.UnPaidRegTotalHours += unpaidDayHours.RegTotalHours;
                                sheet.UnPaidTotalOTHours += unpaidDayHours.TotalOTHours;
                                sheet.UnPaidTotalOT2Hours += unpaidDayHours.TotalOT2Hours;
                            }
                        }

                        // calculate wages for this sheet
                        sheet.RegTotalPay = sheet.RegTotalHours * pay;
                        sheet.TotalOTPay = sheet.TotalOTHours * (pay * 1.5);
                        sheet.TotalOT2Pay = sheet.TotalOT2Hours * (pay * 2);
                        sheet.TotalGrossPay = sheet.RegTotalPay + sheet.TotalOTPay + sheet.TotalOT2Pay;
                    }
                    t.Add(sheet);
                }
            }
            return t;
        }

        public IEnumerable<Employee> GetEmployeeListForProject(int projectId, DateOnly start, DateOnly end)
        {
            return Context.Employee
                .Include(e => e.TimeCards
                        .Where(t => t.ProjectId == projectId
                            && t.TimeCard_Date >= start
                            && t.TimeCard_Date <= end
                            && (t.TimeCard_Status == ShiftStatus.ClockedOut
                            || t.TimeCard_Status == ShiftStatus.Paid)))
                .Where(e => e.Employee_Employed != EmploymentStatus.Deleted)
                .AsEnumerable();
        }

        public async Task<double> GetProjectExpensesAmountAsync(int projectId, DateOnly start, DateOnly end)
        {
            double e = await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.ExpenseTypeId > 1
                    && item.ExpenseTypeId != 3
                    && item.ExpenseTypeId != 4
                    && item.ExpenseDate >= start
                    && item.ExpenseDate <= end)
                .SumAsync(item => item.Amount)
                .ConfigureAwait(false);

            //change from negative # to positive
            return Math.Abs(e);
        }

        public async Task<List<Expense>> GetProjectExpensesListAsync(int projectId, DateOnly start, DateOnly end)
        {
            List<Expense> e = await Context.Expense
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId
                    && item.ExpenseTypeId > 1
                    && item.ExpenseTypeId != 3
                    && item.ExpenseTypeId != 4
                    && item.ExpenseDate >= start
                    && item.ExpenseDate <= end)
                .Distinct()
                .ToListAsync()
                .ConfigureAwait(false);

            return e;
        }

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
