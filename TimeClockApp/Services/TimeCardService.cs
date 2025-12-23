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
            // Get all employed employees with their most recent unpaid timecards in a single query
            var employeesWithCards = await Context.Employee
                .Where(e => e.Employee_Employed == EmploymentStatus.Employed)
                .OrderBy(e => e.Employee_Name)
                .Select(e => new
                {
                    Employee = e,
                    LastTimeCard = Context.TimeCard
                        .Where(tc => tc.EmployeeId == e.EmployeeId
                            && tc.TimeCard_Status < ShiftStatus.Paid
                            && !tc.TimeCard_bReadOnly)
                        .OrderByDescending(tc => tc.TimeCard_DateTime)
                        .FirstOrDefault()
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return employeesWithCards
                .Select(item => item.LastTimeCard ?? new TimeCard(item.Employee))
                .ToList();
        }

        public async Task<bool> EmployeeClockInAsync(TimeCard card, int projectID, int phaseID, string projectName, string phaseTitle)
        {
            if (card.Employee == null || card.EmployeeId == 0 || card.Employee.Employee_Employed != EmploymentStatus.Employed)
                return false;

            if (IsEmployeeNotOnTheClock(card.EmployeeId))
            {
                DateTime entry = DateTime.Now;
                TimeOnly sTime = TimeHelper.RoundTimeOnly(new TimeOnly(entry.Hour, entry.Minute));
                TimeCard c = new(card.Employee, projectID, phaseID, projectName, phaseTitle, sTime);

                Context.Add<TimeCard>(c);
                int saveData = await Context.SaveChangesAsync().ConfigureAwait(false);
                return saveData != 0;
            }
            return false;
        }

        public async Task<bool> EmployeeClockOutAsync(int timeCardId)
        {
            if (timeCardId > 0)
            {
                TimeCard t = GetTimeCard(timeCardId);
                bool valClockOut = await ValidateClockOutAsync(t).ConfigureAwait(false);
                if (valClockOut)
                {
                    t.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
                    t.TimeCard_Status = ShiftStatus.ClockedOut;
                    Context.Update<TimeCard>(t);
                    return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
                }
            }
            return false;
        }

        private bool IsEmployeeNotOnTheClock(int employeeID) => !Context.TimeCard
                .AsNoTracking()
                .Where(tc => tc.EmployeeId == employeeID
                    && tc.TimeCard_Status == ShiftStatus.ClockedIn
                    && !tc.TimeCard_bReadOnly)
                .Any();

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
                    int i = await Context.SaveChangesAsync().ConfigureAwait(false);
                    if (i != 0)
                        return true;
                }
            }
            return false;
        }
    }
}
