using Microsoft.EntityFrameworkCore;
#nullable enable

namespace TimeClockApp.Services
{
	/// <summary>
	/// Provides services for managing application settings.
	/// </summary>
	public class SettingsService : SQLiteDataStore
	{
		/// <summary>
		/// Gets the list of settings asynchronously.
		/// </summary>
		/// <returns>The list of settings</returns>
		public Task<List<Config>> GetSettingsListAsync() => Context.Config.ToListAsync();

		/// <summary>
		/// Saves the configuration asynchronously.
		/// </summary>
		/// <returns>true if the configuration was saved successfully; otherwise, false</returns>
		public async Task<bool> SaveConfigAsync() => await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
	}
}
