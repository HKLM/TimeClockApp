#nullable enable

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
        private const string sQLiteDbFileName = "BaseTimeClock.db3";
#else
        /// <summary>
        /// File name (only) of the SQLite Database used by the app.
        /// </summary>
        /// <remarks>Does not include path.</remarks>
        private const string sQLiteDbFileName = "TimeClockAppDB-2.db3";
#endif
        /// <summary>
        /// Property for the full file path to the SQLite Database
        /// </summary>
        private static string SQLiteDBPath { get; set; } = string.Empty;

        /// <summary>
        /// Method for getting the full file path to SQLite Database
        /// </summary>
        /// <returns>string value of the file path to the SQLite Database</returns>
        public static string GetSQLiteDBPath()
        {
            if (SQLiteDBPath == string.Empty)
            {
#if MIGRATION
                SQLiteDBPath = System.IO.Path.Combine("../", sQLiteDbFileName);
#else
                SQLiteDBPath = System.IO.Path.Combine(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory, sQLiteDbFileName);
#endif
            }
            return SQLiteDBPath;
        }
    }
}
