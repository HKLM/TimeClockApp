namespace TimeClockApp.Services
{
    public class SqliteDataStore : IDisposable
    {
        private protected static readonly DatabackendContext Context = new();

        public SqliteDataStore() { }

        #region POPUP

        /// <summary>
        /// Writes error message to debug log and then shows alert popup to the user, displaying the error message.
        /// </summary>
        /// <param name="errorTxt">Error message to display</param>
        /// <param name="alertTitle">Pop up window title</param>
        public void ShowPopupError(string errorTxt, string alertTitle = "ERROR")
        {
            System.Diagnostics.Debug.WriteLine(errorTxt, alertTitle);
            App.AlertSvc.ShowAlertAsync(alertTitle, errorTxt);
        }
        /// <summary>
        /// Writes error message to debug log and then shows async alert popup to the user, displaying the error message.
        /// </summary>
        /// <param name="errorTxt">Error message to display</param>
        /// <param name="alertTitle">Pop up window title</param>
        public Task ShowPopupErrorAsync(string errorTxt, string alertTitle = "ERROR")
        {
            System.Diagnostics.Debug.WriteLine(errorTxt, alertTitle);
            return App.AlertSvc.ShowAlertAsync(alertTitle, errorTxt);
        }

        #endregion POPUP

        public void Dispose()
        {
            ((IDisposable)Context).Dispose();
        }

        /// <summary>
        /// Gets the INT value of the Config item
        /// </summary>
        /// <param name="keyId">Primary Id key of the item</param>
        /// <param name="defaultValue">1</param>
        /// <returns>The Config's INT value</returns>
        internal int GetConfigInt(int keyId, int defaultValue = 1)
        {
            Config C = Context.Config.Find(keyId);
            return C.IntValue.HasValue ? (int)C.IntValue.Value : defaultValue;
        }

        /// <summary>
        /// Gets the String value of the Config item
        /// </summary>
        /// <param name="keyId">Primary Id key of the item</param>
        /// <returns>the Config's STRING value</returns>
        internal string GetConfigString(int keyId)
        {
            Config C = Context.Config.Find(keyId);
            if (C != null && C.StringValue != null && C.StringValue != "")
                return C.StringValue;
            return null;
        }
    }
}
