using CommunityToolkit.Maui.Core.Extensions;
#nullable enable

namespace TimeClockApp.Services
{
    public class SettingsService : SQLiteDataStore
    {
        public ObservableCollection<Config> GetSettingsList() => Context.Config.ToObservableCollection();

        public bool SaveConfig(Config item)
        {
            string? newStringValue = string.IsNullOrEmpty(item.StringValue) ? null : item.StringValue.Trim();
            Config? C = Context.Config.Find(item.ConfigId);
            if (C != null)
            {
                C.IntValue = item.IntValue ?? null;
                C.StringValue = newStringValue;
                Context.Update<Config>(C);
                return Context.SaveChanges() > 0;
            }
            return false;
        }
    }
}
