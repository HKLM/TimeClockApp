using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Core.Extensions;

using TimeClockApp.Models;

namespace TimeClockApp.Services
{
    public class SettingsService : SqliteDataStore
    {
        public ObservableCollection<Config> GetSettingsList() => Context.Config.ToObservableCollection();

        public bool SaveConfig(Config item)
        {
            Config C = Context.Config.Find(item.ConfigId);
            if (C != null)
            {
                C.IntValue = item.IntValue ?? null;
                C.StringValue = item.StringValue ?? null;
                Context.Update<Config>(C);
                return Context.SaveChanges() > 0;
            }
            return false;
        }
    }
}
