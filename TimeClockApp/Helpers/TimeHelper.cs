namespace TimeClockApp.Helpers
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
    }
}
