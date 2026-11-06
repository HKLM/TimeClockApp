namespace TimeClockApp.Shared
{
	/// <summary>
	/// Class holds the FilePath + filename of the SQLite database.
	/// </summary>
	public static class SQLiteSetting
	{
#if MIGRATION
        /// <summary>
        /// File name (only) of the SQLite Database used by the EFMigrator for setting up EF migrations.
        /// </summary>
        /// <remarks>Does not include path.</remarks>
        public const string sQLiteDbFileName = "BaseTimeClock.db3";
        public static string GetSQLiteDBPath => System.IO.Path.Combine("../", sQLiteDbFileName);
#else
		/// <summary>
		/// File name (only) of the SQLite Database used by the app.
		/// </summary>
		/// <remarks>Does not include path.</remarks>
		public const string sQLiteDbFileName = "TimeCardAppDB-v4.db3";

		/// <summary>
		/// Gets the full file path to the SQLite Database.
		/// </summary>
		public static string GetSQLiteDBPath => System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, sQLiteDbFileName);
#endif
	}
}
