using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared.Helpers;

#nullable enable
namespace TimeClockApp.Services
{
    public class EditTimeCardService : TimeCardDataStore
    {
        public TimeCard? GetTimeCardByID(int cardId) =>
            Context.TimeCard
                .Include(item => item.Employee)
                .Where(item => item.TimeCardId == cardId
                    && item.TimeCard_Status != ShiftStatus.Deleted)
                .FirstOrDefault();

        public async Task<List<TimeCard>> GetTimeCards(int NumResults)
        {
            return await Context.TimeCard
                .AsNoTracking()
                .Where(item => item.TimeCard_Status != ShiftStatus.Deleted)
                .OrderByDescending(item => item.TimeCard_DateTime)
                .Take(NumResults)
                .ToListAsync();
        }

        public bool UpdateTimeCard(TimeCard newTimeCard, bool isAdmin = false, bool bChangedDate = false)
        {
            if (newTimeCard == null)
                return false;

            if (IsTimeCardReadOnly(newTimeCard.TimeCardId, isAdmin))
                return false;

            DateOnly newDate = new();
            TimeCard? origTimeCard = Context.TimeCard.Find(newTimeCard.TimeCardId);
            if (origTimeCard != null)
            {
                origTimeCard.PhaseId = newTimeCard.PhaseId;
                origTimeCard.PhaseTitle = newTimeCard.PhaseTitle;
                origTimeCard.ProjectId = newTimeCard.ProjectId;
                origTimeCard.ProjectName = newTimeCard.ProjectName;
                origTimeCard.TimeCard_Status = newTimeCard.TimeCard_Status;
                origTimeCard.TimeCard_bReadOnly = newTimeCard.TimeCard_bReadOnly;
                origTimeCard.TimeCard_StartTime = TimeHelper.RoundTimeOnly(new TimeOnly(newTimeCard.TimeCard_StartTime.Hour, newTimeCard.TimeCard_StartTime.Minute));
                origTimeCard.TimeCard_EndTime = TimeHelper.RoundTimeOnly(new TimeOnly(newTimeCard.TimeCard_EndTime.Hour, newTimeCard.TimeCard_EndTime.Minute));

                newDate = newTimeCard.TimeCard_Date;
                if (bChangedDate)
                {
                    origTimeCard.TimeCard_Date = newDate;
                    if (DateOnly.FromDateTime(origTimeCard.TimeCard_DateTime) != newDate)
                        origTimeCard.TimeCard_DateTime = new DateTime(newTimeCard.TimeCard_Date.Year, newTimeCard.TimeCard_Date.Month, newTimeCard.TimeCard_Date.Day, newTimeCard.TimeCard_DateTime.Hour, newTimeCard.TimeCard_DateTime.Minute, newTimeCard.TimeCard_DateTime.Second);
                }

                Context.Update<TimeCard>(origTimeCard);
                if (Context.SaveChanges() > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
