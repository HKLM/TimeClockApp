#nullable enable

using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Services
{
    public class SettingsService : SQLiteDataStore
    {
        public Task<List<Config>> GetSettingsListAsync() => Context.Config.ToListAsync();

        public async Task<bool> SaveConfigAsync(Config item)
        {
            string? newStringValue = string.IsNullOrEmpty(item.StringValue) ? null : item.StringValue.Trim();
            Config? C = await Context.Config.FindAsync(item.ConfigId);
            if (C != null)
            {
                C.IntValue = item.IntValue ?? null;
                C.StringValue = newStringValue;
                Context.Update<Config>(C);
                return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
            }
            return false;
        }
    }
}
