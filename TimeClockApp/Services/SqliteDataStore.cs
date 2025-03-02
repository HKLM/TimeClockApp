#nullable enable

namespace TimeClockApp.Services
{
    public class SQLiteDataStore
    {
        private protected static readonly DataBackendContext Context = new();

        public SQLiteDataStore() { }

#region POPUP

        /// <summary>
        /// Writes error message to debug log and then shows alert popup to the user, displaying the error message.
        /// </summary>
        /// <param name="errorTxt">Error message to display</param>
        /// <param name="alertTitle">Pop up window title</param>
        public void ShowPopupError(string errorTxt, string alertTitle = "ERROR")
        {
            Log.WriteLine(errorTxt, alertTitle);
            App.AlertSvc!.ShowAlertAsync(alertTitle, errorTxt);
        }
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

        internal async Task<int> GetConfigIntAsync(int keyId, int defaultValue = 1)
        {
            Config? C = await Context.Config.FindAsync(keyId).ConfigureAwait(false);
            return C?.IntValue ?? defaultValue;
        }


        /// <summary>
        /// Gets the String value of the Config item
        /// </summary>
        /// <param name="keyId">Primary Id key of the item</param>
        /// <returns>the Config's STRING value</returns>
        internal string GetConfigString(int keyId)
        {
            Config? C = Context.Config.Find(keyId);
            return C?.StringValue ?? string.Empty;
        }

        internal async Task<string> GetConfigStringAsync(int keyId)
        {
            Config? C = await Context.Config.FindAsync(keyId).ConfigureAwait(false); ;
            return C?.StringValue ?? string.Empty;
        }
    }
}
