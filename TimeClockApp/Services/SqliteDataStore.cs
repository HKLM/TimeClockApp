using TimeClockApp.Shared;
#nullable enable

namespace TimeClockApp.Services
{
    /// <summary>
    /// Provides base data access functionality for the SQLite database.
    /// </summary>
    public class SQLiteDataStore
    {
        private protected static readonly DataBackendContext Context = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDataStore"/> class.
        /// </summary>
        public SQLiteDataStore() { }

        /// <summary>
        /// Gets the date when the app was first run.
        /// </summary>
        /// <returns></returns>
        public DateTime GetAppFirstRunDate()
        {
            bool bFound = false;
            DateOnly d = DateOnly.MinValue;
            if (Preferences.ContainsKey("FirstRun", PrefSetting.PrefKey))
            {
                bFound = DateOnly.TryParse(Preferences.Get("FirstRun", DateOnly.MinValue.ToShortDateString(), PrefSetting.PrefKey), out d);
            }
            if (!bFound)
            {
                int i = 2;
                Config? c = Context.Config.Find(i);
                if (c != null)
                {
                    d = DateOnly.Parse(c.StringValue!);
                    Preferences.Set("FirstRun", c.StringValue, PrefSetting.PrefKey);
                }
            }
            d = d.AddDays(-6);
            return d.ToDateTime(new TimeOnly(0, 0));
        }

#region POPUP

        /// <summary>
        /// Writes error message to debug log and then shows async alert popup to the user, displaying the error message.
        /// </summary>
        /// <param name="errorTxt">Error message to display</param>
        /// <param name="alertTitle">Pop up window title</param>
        public Task ShowPopupErrorAsync(string errorTxt, string alertTitle = "ERROR")
        {
            Log.WriteLine(errorTxt, alertTitle);
            return App.AlertSvc!.ShowAlertAsync(alertTitle, errorTxt);
        }

#endregion POPUP

        /// <summary>
        /// Gets the INT value of the Config item
        /// </summary>
        /// <param name="keyId">Primary Id key of the item</param>
        /// <param name="defaultValue">1</param>
        /// <returns>The Config's INT value</returns>
        internal int GetConfigInt(int keyId, int defaultValue = 1)
        {
            Config? C = Context.Config.Find(keyId);
            return C?.IntValue ?? defaultValue;
        }
    }
}
