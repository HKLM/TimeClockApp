using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared;
using TimeClockApp.Shared.Helpers;

namespace TimeClockApp.Services
{
    /// <summary>
    /// Provides data access functionality for time cards.
    /// </summary>
    public class TimeCardDataStore : SQLiteDataStore
    {
        /// <summary>
        /// returns the first previous Saturday before the <paramref name="day"/>
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public DateTime GetStartOfPayPeriod(DateTime day)
        {
            DayOfWeek p = GetPayDayOfWeek();
            if (day.DayOfWeek == p)
                return DateTime.Now.AddDays(-7);

            while (day.DayOfWeek != p)
            {
                day = day.AddDays(-1);
            }
            return day;
        }

        private DayOfWeek GetPayDayOfWeek()
        {
            int x;
            int p = -1;
            if (Preferences.ContainsKey("PayDayOfWeek", PrefSetting.PrefKey))
            {
                p = Preferences.Get("PayDayOfWeek", 1, PrefSetting.PrefKey);
            }
            if (p == -1)
            {
                int i = 11;
                Config c = Context.Config.Find(i);
                x = c.IntValue.Value + 1 == 7 ? 0 : c.IntValue.Value;
            }
            else
            {
                x = p + 1 == 7 ? 0 : p;
            }
            return (DayOfWeek)x;
        }

#region PROJECTS_PHASES
        public ObservableCollection<Phase> GetPhaseList() => Context.Phase.OrderBy(e => e.PhaseTitle).ToObservableCollection();

        /// <summary>
        /// Gets the current project entity.
        /// </summary>
        /// <returns></returns>
        public Project GetCurrentProjectEntity()
        {
            int? c = Preferences.Get("CurrentProject", -1, PrefSetting.PrefKey);
            if (c == -1)
            {
                c = GetConfigInt(3, 1);
                if (!c.HasValue || c.Value == 0)
                    c = 1;
            }
            return Context.Project.Find(c.Value);
        }

        /// <summary>
        /// Gets the current phase entity.
        /// </summary>
        /// <returns></returns>
        public Phase GetCurrentPhaseEntity()
        {
            int? c = Preferences.Get("CurrentPhase", -1, PrefSetting.PrefKey);
            if (c == -1)
            {
                c = GetConfigInt(4, 1);
                if (!c.HasValue || c.Value == 0)
                    c = 1;
            }
            return Context.Phase.Find(c.Value);
        }

		/// <summary>
		/// Saves the current project projectId.
		/// </summary>
		/// <param name="projectId"></param>
		/// <returns></returns>
		public async Task SaveCurrentProjectAsync(int projectId)
        {
            Preferences.Set("CurrentProject", projectId, PrefSetting.PrefKey);
            int i = 3;
            var C = Context.Config.Find(i);
            if (C.IntValue.HasValue && C.IntValue.Value != projectId)
            {
                C.IntValue = projectId;
                Context.Update<Config>(C);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

		/// <summary>
		/// Saves the current phase by phaseId.
		/// </summary>
		/// <param name="phaseId"></param>
		/// <returns></returns>
		public async Task SaveCurrentPhaseAsync(int phaseId)
        {
            Preferences.Set("CurrentPhase", phaseId, PrefSetting.PrefKey);
            int i = 4;
            Config C = Context.Config.Find(i);
            if (C.IntValue.HasValue && C.IntValue.Value != phaseId)
            {
                C.IntValue = phaseId;
                Context.Update<Config>(C);
                await Context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

#endregion

#region TIMECARDS
        /// <summary>
        /// Gets the time card by its ID.
        /// </summary>
        /// <param name="timeCardID"></param>
        /// <returns></returns>
        public TimeCard GetTimeCard(int timeCardID) => Context.TimeCard.Find(timeCardID);

        /// <summary>
        /// Gets the time card by its ID asynchronously.
        /// </summary>
        /// <param name="timeCardID"></param>
        /// <returns></returns>
		public async Task<TimeCard> GetTimeCardAsync(int timeCardID) => await Context.TimeCard.FindAsync(timeCardID);

		/// <summary>
		/// Checks if the time card has the read only flag set
		/// </summary>
		/// <param name="timeCardId">Id of time card to check</param>
		/// <param name="isAdmin">if IsAdmin is set true, override the ReadOnly flag and allow edits. Defaults to false</param>
		/// <returns></returns>
		public bool IsTimeCardReadOnly(int timeCardId, bool isAdmin = false) =>
            !isAdmin && Context.TimeCard
                .AsNoTracking()
                .Where(tc => tc.TimeCardId == timeCardId)
                .Select(tc => tc.TimeCard_bReadOnly)
                .FirstOrDefault();

        /// <summary>Basic validation of the timecard. Checks that the end time is before start time. 
        /// If time card is set to a time in the future. If the time card was on a prior date and has not been clocked out.
        /// Does not check for overlapping times.</summary>
        /// <param name="timeCard"></param>
        /// <returns>true if the time card is valid for clocking out, false otherwise</returns>
        internal async Task<bool> ValidateClockOutAsync(TimeCard timeCard)
        {
            if (timeCard == null || timeCard.TimeCardId == 0)
                return false;

            TimeOnly clockNowRndTime = TimeHelper.RoundTimeOnly(new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute));
            DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
            int dateCheck = timeCard.TimeCard_Date.CompareTo(dateNow);
            if (dateCheck < 0)
            {
                Log.WriteLine("ERROR! Card Time needs to be checked before saving. Most likely this card was ClockedIn on a prior date and not clockedOut on said date.");
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}&e=1");
                return false;
            }
            else if (timeCard.TimeCard_StartTime > TimeOnly.FromDateTime(DateTime.Now))
            {
                if (TimeHelper.CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime))
                    return true;

                Log.WriteLine("ERROR! Card Time start time is greater than the current time.");
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}&e=2");
                return false;
            }
            else if (dateCheck == 0)
            {
                return ValidatingClockOutTimes(timeCard.TimeCard_StartTime, TimeOnly.FromDateTime(DateTime.Now)) || TimeHelper.CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime);
            }
            //timecard is in the future
            else if (dateCheck > 0)
            {
                Log.WriteLine($"ERROR! TimeCard Date is in the future: {timeCard.TimeCard_Date:d}\nTIMECARD REDIRECTING TO EDIT TIMECARD PAGE");
                //ERROR CODE V2
                await App.AlertSvc!.ShowAlertAsync("ERROR!", $"ERROR! TimeCard Date is in the future: {timeCard.TimeCard_Date:d}\nCan not save TimeCard until this is corrected.").ConfigureAwait(false);
                await Shell.Current.GoToAsync($"EditTimeCard?id={timeCard.TimeCardId}");
                // refresh the timecard, and recheck if issue is fixed
                timeCard = GetTimeCard(timeCard.TimeCardId);
                dateCheck = timeCard.TimeCard_Date.CompareTo(dateNow);
                if (dateCheck < 0)
                {
                    TimeOnly timeEndVar = timeCard.TimeCard_EndTime == new TimeOnly() ? TimeOnly.FromDateTime(DateTime.Now) : timeCard.TimeCard_EndTime;
                    return ValidatingClockOutTimes(timeCard.TimeCard_StartTime, timeEndVar);
                }
                else if (dateCheck == 0)
                {
                    bool b = TimeHelper.CheckIfTimeIsCloseToNow(clockNowRndTime, timeCard.TimeCard_StartTime);
                    bool vb = ValidatingClockOutTimes(timeCard.TimeCard_StartTime, TimeOnly.FromDateTime(DateTime.Now));
                    return b && vb;
                }
                return false;
            }

            return true;
        }

        private bool ValidatingClockOutTimes(TimeOnly startTime, TimeOnly endTime) => endTime.CompareTo(startTime) >= 0;

        //private bool CheckIfTimeIsCloseToNow(TimeOnly clockNowRndTime, TimeOnly timeStart) => clockNowRndTime - timeStart < new TimeSpan(0, 15, 0);

#endregion

#region EXPENSES

        private string GetExpenseType_CategoryName(int expenseTypeId) => Context.ExpenseType.First(z => z.ExpenseTypeId == expenseTypeId).CategoryName;

        // used to add new entry when marking a timecard as paid
        public async Task<bool> AddNewExpenseAsync(int projectId, int phaseId, double amount, string memo, string projectName, string phaseTitle, int expenseTypeId = 3, string expenseTypeCategoryName = "")
        {
            string e = string.IsNullOrEmpty(expenseTypeCategoryName) ? GetExpenseType_CategoryName(expenseTypeId) : expenseTypeCategoryName;

            //make expenses (not income) a negative number
            if (expenseTypeId != 2)
                amount *= (-1);

            Expense exp = new(projectId, phaseId, amount, DateOnly.FromDateTime(DateTime.Now), projectName, phaseTitle, memo, expenseTypeId, e);

            Context.Add<Expense>(exp);
            var result = await Context.SaveChangesAsync().ConfigureAwait(false);
			return result > 0;
        }
#endregion
    }
}
