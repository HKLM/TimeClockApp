namespace TimeClockApp.Shared
{
    public static class PrefSetting
    {
        /// <summary>
        /// Gets the shared preference key for the time card application.
        /// </summary>
        private static readonly string _PrefConfigKey = "timecard_app";
        public static string PrefKey => _PrefConfigKey;
    }
}
