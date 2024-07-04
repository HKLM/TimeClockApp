using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

#nullable enable

namespace TimeClockApp.Services
{
    public class ReportPageService : PayrollService
    {
        [RequiresUnreferencedCode("Enumerating in-memory collections as IQueryable can require unreferenced code because expressions referencing IQueryable extension methods can get rebound to IEnumerable extension methods. The IEnumerable extension methods could be trimmed causing the application to fail at runtime.")]
        public async Task<List<TimeSheet>> RunFullReportAsync(bool useEmployeeFilter, bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, Employee? employee, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            List<TimeSheet> t = [];
            List<Employee> e = [];
            if (useEmployeeFilter && employee != null)
            {
                Employee employ = GetEmployee(employee.EmployeeId);
                e.Add(employ);
            }
            else
            {
               e = GetEmployeesList();
            }
            start ??= DateOnly.FromDateTime(GetAppFirstRunDate());
            end ??= DateOnly.FromDateTime(DateTime.Now);
            foreach (Employee emp in e)
            {
                TimeSheet sheet = new TimeSheet(emp.EmployeeId, start.Value, end.Value, emp.Employee_Name);
                sheet.TimeCards = await ReportListPaidTimeCardsForPayPeriodAsync(useProjectFilter, usePhaseFilter, useDateFilter, emp, project, phase, start, end);

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

        public List<Employee> GetEmployeesList() => Context.Employee.AsNoTrackingWithIdentityResolution()
            .Where(e => e.Employee_Employed != EmploymentStatus.Deleted)
            .ToList();

        protected async Task<List<TimeCard>> ReportListPaidTimeCardsForPayPeriodAsync(bool useProjectFilter, bool usePhaseFilter, bool useDateFilter, Employee employee, Project? project, Phase? phase, DateOnly? start, DateOnly? end)
        {
            IQueryable<TimeCard> q = Context.TimeCard.AsNoTrackingWithIdentityResolution().AsQueryable();
            q = q.Distinct()
                .Where(item => item.EmployeeId == employee.EmployeeId
                    && (item.TimeCard_Status == ShiftStatus.ClockedOut
                    || item.TimeCard_Status == ShiftStatus.Paid));

            if (useProjectFilter && project != null)
            {
                q = q.Where(item => item.ProjectId == project.ProjectId);
            }
            if (useProjectFilter && usePhaseFilter && project != null && phase != null)
            {
                q = q.Where(item =>
                        item.ProjectId == project.ProjectId
                        && item.PhaseId == phase.PhaseId);
            }
            if (useDateFilter && start.HasValue && end.HasValue)
            {
                q = q.Where(item =>
                        item.TimeCard_Date >= start!.Value
                     && item.TimeCard_Date <= end!.Value);
            }

            return await q.Distinct().ToListAsync();
        }
    }
}
