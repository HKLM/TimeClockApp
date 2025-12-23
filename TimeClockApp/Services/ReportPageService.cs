using Microsoft.EntityFrameworkCore;

#nullable enable

namespace TimeClockApp.Services
{
    public class ReportPageService : PayrollService
    {
        public async Task<List<TimeSheet>> RunFullReportAsync(bool useEmployeeFilter, bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, List<Employee> employeeList, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            List<TimeSheet> t = [];
            if (!useEmployeeFilter || !employeeList.Any())
            {
                employeeList = GetEmployeesList();
            }
            start ??= DateOnly.FromDateTime(GetAppFirstRunDate());
            end ??= DateOnly.FromDateTime(DateTime.Now);
            foreach (Employee emp in employeeList)
            {
                TimeSheet sheet = new TimeSheet(emp.EmployeeId, start.Value, end.Value, emp.Employee_Name);
                sheet.TimeCards = await ReportListPaidTimeCardsForPayPeriodAsync(useProjectFilter, usePhaseFilter, useDateFilter, emp, project, phase, start, end).ConfigureAwait(false);

                if (sheet.TimeCards.Count > 0)
                {
                    double pay = 0;
                    IQueryable<IGrouping<DateOnly, TimeCard>> tg = sheet.TimeCards.GroupBy(x => x.TimeCard_Date).AsQueryable();
                    foreach (IGrouping<DateOnly, TimeCard> g in tg)
                    {
                        double x = 0;
                        foreach (TimeCard item in g)
                        {
                            if (item.TimeCard_Status == ShiftStatus.ClockedOut ||
                                item.TimeCard_Status == ShiftStatus.Paid)
                            {
                                x += item.TimeCard_WorkHours;
                            }
                        }

                        //TODO fix situation if being paid a differant amount. e.g. paid X amount on monday-wed, gets a raise to Z amount on thurs. Currently it will then pay out all days as Z amount.
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
                    }

                    // calculate wages for this sheet
                    sheet.RegTotalPay = sheet.RegTotalHours * pay;
                    sheet.TotalOTPay = sheet.TotalOTHours * (pay * 1.5);
                    sheet.TotalOT2Pay = sheet.TotalOT2Hours * (pay * 2);
                    sheet.TotalGrossPay = sheet.RegTotalPay + sheet.TotalOTPay + sheet.TotalOT2Pay;
                }
                t.Add(sheet);
            }
            return t;
        }

        public List<Employee> GetEmployeesList() => Context.Employee
            .AsNoTrackingWithIdentityResolution()
            .Where(e => e.Employee_Employed != EmploymentStatus.Deleted)
            .ToList();

        protected Task<List<TimeCard>> ReportListPaidTimeCardsForPayPeriodAsync(bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, Employee employee, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            IQueryable<TimeCard> q = Context.TimeCard
                .TagWith("ReportListPaidTimeCardsForPayPeriodAsync")
                .AsNoTrackingWithIdentityResolution()
                .AsQueryable();

            q = q
                .Where(item => item.EmployeeId == employee.EmployeeId
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

            return q.Distinct().OrderBy(item => item.TimeCard_Date).ToListAsync();
        }
    }
}
