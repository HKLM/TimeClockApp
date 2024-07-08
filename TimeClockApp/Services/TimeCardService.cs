using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared.Helpers;

#nullable enable

namespace TimeClockApp.Services
{
    public class TimeCardService : TimeCardDataStore
    {
        public async Task<List<TimeCard>> GetLastTimeCardForAllEmployeesAsync()
        {
            List<TimeCard> _timeCards = new();

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

                _timeCards.Add(x ?? new TimeCard(employee));
            }
            return _timeCards;
        }

        public async Task<bool> EmployeeClockInAsync(TimeCard card, int projectID, int phaseID, string projectName, string phaseTitle)
        {
            if (card.Employee == null || card.EmployeeId == 0 || card.Employee.Employee_Employed != EmploymentStatus.Employed)
                return false;

            if (IsEmployeeNotOnTheClock(card.EmployeeId))
            {
                DateTime entry = DateTime.Now;
                TimeCard c = new(card.Employee, projectID, phaseID, projectName, phaseTitle)
                {
                    TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(entry.Hour, entry.Minute)),
                    TimeCard_Status = ShiftStatus.ClockedIn
                };

                Context.Add<TimeCard>(c);
                Task<int> saveData = Context.SaveChangesAsync();
                return await saveData != 0;
            }
            return false;
        }

        public async Task<bool> EmployeeClockOutAsync(TimeCard timeCard)
        {
            if (timeCard != null && timeCard.TimeCardId != 0)
            {
                TimeCard t = GetTimeCard(timeCard.TimeCardId);
                Task<bool> valClockOut = ValidateClockOutAsync(t);
                if (await valClockOut)
                {
                    t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
                    t.TimeCard_Status = ShiftStatus.ClockedOut;
                    Context.Update<TimeCard>(t);
                    Task<int> saveData = Context.SaveChangesAsync();
                    if (await saveData > 0)
                    {
                        return true;
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

        public async Task<bool> MarkTimeCardAsPaidAsync(TimeCard timeCard)
        {
            if (timeCard != null && timeCard.TimeCardId != 0 
                && !IsTimeCardReadOnly(timeCard.TimeCardId) 
                && timeCard.TimeCard_Status == ShiftStatus.ClockedOut)
            {
                TimeCard? t = Context.TimeCard.Find(timeCard.TimeCardId);
                if (t != null)
                {
                    t.TimeCard_Status = ShiftStatus.Paid;
                    t.TimeCard_bReadOnly = true;
                    Context.Update<TimeCard>(t);
                    Task<int> i = Context.SaveChangesAsync();
                    if (await i != 0)
                        return true;
                }
            }
            return false;
        }
    }
}
