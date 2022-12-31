using System.Collections.ObjectModel;
using System.Data;

using Microsoft.EntityFrameworkCore;

using TimeClockApp.Helpers;
using TimeClockApp.Models;

namespace TimeClockApp.Services
{
    public partial class TimeCardService : TimeCardDataStore
    {
        public async Task<double> GetHourTotalForEmployeecAsync(int employeeId, DateOnly start, DateOnly end)
        {
            IQueryable<TimeCard> t = Context.TimeCard
                .AsNoTracking()
                .Where(item => item.EmployeeId == employeeId
                    && (item.TimeCard_Date >= start
                    && item.TimeCard_Date <= end)
                    && item.TimeCard_Status == ShiftStatus.ClockedOut
                    && !item.TimeCard_bReadOnly);

            return await t.AnyAsync() ? await t.SumAsync(o => o.TimeCard_WorkHours) : 0;
        }

#nullable enable

        public async Task<ObservableCollection<TimeCard>> GetLastTimeCardForAllEmployeesAsync()
        {
            DateOnly payrollPeriod = DateOnly.FromDateTime(GetStartOfPayPeriod(DateTime.Now));
            DateOnly d = DateOnly.FromDateTime(DateTime.Now);
            ObservableCollection<TimeCard> _timeCards = new();

            IOrderedQueryable<Employee> emp = Context.Employee
                 .Where(e => e.Employee_Employed == EmploymentStatus.Employed)
                 .OrderBy(e => e.Employee_Name);

            foreach (Employee employee in emp)
            {
                TimeCard? x = await Context.TimeCard
                            .Where(item => item.EmployeeId == employee.EmployeeId
                                && (item.TimeCard_Status < ShiftStatus.Paid
                                && !item.TimeCard_bReadOnly))
                            .OrderByDescending(item => item.TimeCard_DateTime)
                            .FirstOrDefaultAsync();

                if (x != null)
                {
                    x.TotalWorkHours = await GetHourTotalForEmployeecAsync(employee.EmployeeId, payrollPeriod, d);
                    _timeCards.Add(x);
                }
                else
                    _timeCards.Add(new TimeCard(employee));
            }
            return _timeCards;
        }

#nullable disable

        public async Task<bool> EmployeeClockInAsync(TimeCard card, int projectID, int phaseID)
        {
            if (card.Employee == null || card.EmployeeId == 0 || card.Employee.Employee_Employed != EmploymentStatus.Employed)
                return false;

            if (IsEmployeeNotOnTheClock(card.EmployeeId))
            {
                DateTime entry = DateTime.Now;
                TimeCard c = new(card.Employee, projectID, phaseID)
                {
                    TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(entry.Hour, entry.Minute)),
                    TimeCard_Status = ShiftStatus.ClockedIn
                };

                Context.Add<TimeCard>(c);
                return await Context.SaveChangesAsync() != 0;
            }
            return false;
        }

        public async Task<bool> EmployeeClockOutAsync(TimeCard timeCard)
        {
            if (timeCard != null && timeCard.TimeCardId != 0)
            {
                TimeCard t = GetTimeCard(timeCard.TimeCardId);
                if (await ValidateClockOutAsync(t))
                {
                    t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
                    t.TimeCard_Status = ShiftStatus.ClockedOut;
                    Context.Update<TimeCard>(t);
                    if (await Context.SaveChangesAsync() > 0)
                    {
                        return await CalculatePayAsync(t);
                    }
                }
            }
            return false;
        }

        private bool IsEmployeeNotOnTheClock(int employeeID)
        {
            return !Context.TimeCard
                .AsNoTracking()
                .Where(tc => tc.EmployeeId == employeeID
                    && !tc.TimeCard_bReadOnly
                    && tc.TimeCard_Status == ShiftStatus.ClockedIn)
                .Any();
        }

        private async Task<bool> IsEmployeeNotOnTheClockAsync(int employeeID)
        {
            bool b = await Context.TimeCard
                .AsNoTracking()
                .Where(tc => tc.EmployeeId == employeeID
                    && !tc.TimeCard_bReadOnly
                    && tc.TimeCard_Status == ShiftStatus.ClockedIn)
                .AnyAsync();
            return !b;
        }
    }
}
