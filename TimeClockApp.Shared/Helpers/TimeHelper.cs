using System.Globalization;

namespace TimeClockApp.Shared.Helpers
{
    public static class TimeHelper
    {
        private static DateTime RoundDateTime(DateTime dt, TimeSpan d)
        {
            int f = 0;
            double m = (double)(dt.Ticks % d.Ticks) / d.Ticks;
            if (m >= 0.5)
                f = 1;
            return new DateTime(((dt.Ticks / d.Ticks) + f) * d.Ticks);
        }

        /// <summary>
        /// Rounds the time input to nearest 15 min interval
        /// </summary>
        /// <param name="time"></param>
        /// <returns>TimeOnly</returns>
        public static TimeOnly RoundTimeOnly(TimeOnly time)
        {
            DateTime dt = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, time.Hour, time.Minute, time.Second);
            DateTime roundTime = RoundDateTime(dt, TimeSpan.FromMinutes(15));
            return TimeOnly.FromDateTime(roundTime);
        }
        /// <summary>
        /// Rounds the nullable time input to nearest 15 min interval
        /// </summary>
        /// <param name="time"></param>
        /// <returns>nullable TimeOnly</returns>
        public static TimeOnly? RoundTimeOnly(TimeOnly? time)
        {
            if (time == null) return null;
            DateTime dt = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, time.Value.Hour, time.Value.Minute, time.Value.Second);
            DateTime? roundTime = RoundDateTime(dt, TimeSpan.FromMinutes(15));
            return TimeOnly.FromDateTime(roundTime.Value);
        }

        /// <summary>
        /// Gets the weeks number for the given date
        /// </summary>
        /// <param name="thisDate">The date to get the week number for</param>
        /// <returns>the week number</returns>
        public static int GetWeekNumber(DateTime thisDate) =>
            CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(thisDate, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Saturday);

        public static int GetWeekNumber(DateOnly thisDate) =>
            CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(new DateTime(thisDate.Year, thisDate.Month, thisDate.Day), System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
    }
}
