using System.Data;
using Microsoft.EntityFrameworkCore;
using TimeClockApp.Shared;
#nullable enable

namespace TimeClockApp.Services
{
    public class SQLiteDataStore
    {
        private protected static readonly DataBackendContext Context = new();

        public SQLiteDataStore() { }

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

        public async Task<List<Phase>> GetPhaseListAsync() => await Context.Phase.OrderBy(e => e.PhaseTitle).ToListAsync();

        public async Task<List<Project>> GetProjectsListAsync() =>
            await Context.Project
                .Where(item => item.Status < ProjectStatus.Completed)
                .OrderBy(e => e.Name)
                .ToListAsync();

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
