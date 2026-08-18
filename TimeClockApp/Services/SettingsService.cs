using Microsoft.EntityFrameworkCore;
#nullable enable

namespace TimeClockApp.Services
{
	public class SettingsService : SQLiteDataStore
	{
		public Task<List<Config>> GetSettingsListAsync() => Context.Config.ToListAsync();

		public async Task<bool> SaveConfigAsync() => await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
	}
}
