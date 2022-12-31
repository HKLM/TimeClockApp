using System.Collections.ObjectModel;
using System.Data;

using CommunityToolkit.Maui.Core.Extensions;

using Microsoft.EntityFrameworkCore;

using TimeClockApp.Helpers;
using TimeClockApp.Models;

namespace TimeClockApp.Services
{
    public partial class EditTimeCardService : TimeCardDataStore
    {
        public async Task<Phase> GetPhaseAsync(int phaseID) => await Context.Phase.FindAsync(phaseID);

        public async Task<Project> GetProjectAsync(int projectID) => await Context.Project.FindAsync(projectID);

        public Task<TimeCard> GetTimeCardByIDAsync(int cardId) => Context.TimeCard
                .Include(item => item.Employee)
                .Where(item => item.TimeCardId == cardId
                    && item.TimeCard_Status != ShiftStatus.Deleted)
                .FirstAsync();

        public TimeCard GetTimeCardByID(int cardId) => Context.TimeCard
                .Include(item => item.Employee)
                .Where(item => item.TimeCardId == cardId
                    && item.TimeCard_Status != ShiftStatus.Deleted)
                .FirstOrDefault();

        public ObservableCollection<TimeCard> GetTimeCardsForEmployee(int employeeId, bool bShowPaid = false)
        {
            ShiftStatus s;
            if (bShowPaid)
            {
                s = ShiftStatus.Deleted;
                return Context.TimeCard
                    .AsNoTracking()
                    //.Include(item => item.Employee)
                    .Where(item => item.EmployeeId == employeeId
                        && item.TimeCard_Status != s)
                    .OrderByDescending(item => item.TimeCard_DateTime)
                    .ToObservableCollection();
            }
            else
            {
                s = ShiftStatus.Paid;
                return Context.TimeCard
                    .AsNoTracking()
                    //.Include(item => item.Employee)
                    .Where(item => item.EmployeeId == employeeId
                        && item.TimeCard_Status < s
                        && !item.TimeCard_bReadOnly)
                    .OrderByDescending(item => item.TimeCard_DateTime)
                    .ToObservableCollection();
            }
        }

        public bool UpdateTimeCard(TimeCard newTimeCard)
        {
            if (newTimeCard == null)
                return false;

            if (IsTimeCardReadOnly(newTimeCard.TimeCardId))
                return false;

            TimeCard origTimeCard = Context.TimeCard.Find(newTimeCard.TimeCardId);
            if (origTimeCard != null)
            {
                origTimeCard.PhaseId = newTimeCard.PhaseId;
                origTimeCard.PhaseTitle = newTimeCard.PhaseTitle;
                origTimeCard.ProjectId = newTimeCard.ProjectId;
                origTimeCard.ProjectName = newTimeCard.ProjectName;
                origTimeCard.TimeCard_Status = newTimeCard.TimeCard_Status;
                origTimeCard.TimeCard_bReadOnly = newTimeCard.TimeCard_bReadOnly;
                origTimeCard.TimeCard_Date = newTimeCard.TimeCard_Date;
                origTimeCard.TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(newTimeCard.TimeCard_StartTime.Hour, newTimeCard.TimeCard_StartTime.Minute));
                origTimeCard.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(newTimeCard.TimeCard_EndTime.Hour, newTimeCard.TimeCard_EndTime.Minute));
                //update TimeCard_DateTime only if TimeCard_Date has changed.
                if (DateOnly.FromDateTime(origTimeCard.TimeCard_DateTime) != newTimeCard.TimeCard_Date)
                    origTimeCard.TimeCard_DateTime = new DateTime(newTimeCard.TimeCard_Date.Year, newTimeCard.TimeCard_Date.Month, newTimeCard.TimeCard_Date.Day, newTimeCard.TimeCard_DateTime.Hour, newTimeCard.TimeCard_DateTime.Minute, newTimeCard.TimeCard_DateTime.Second);

                Context.Update<TimeCard>(origTimeCard);
                if (Context.SaveChanges() > 0)
                {
                    CalculatePay(newTimeCard);
                    return true;
                }
            }
            return false;
        }
    }
}
