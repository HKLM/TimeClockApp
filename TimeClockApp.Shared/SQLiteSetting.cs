using System.IO;
using Microsoft.Maui.Storage;

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
        private const string SQLiteDbFileName = "BaseTimeClock.db3";
#else
        /// <summary>
        /// File name (only) of the SQLite Database used by the app.
        /// </summary>
        /// <remarks>Does not include path.</remarks>
        private const string SQLiteDbFileName = "TimeClockAppDB-04.db3";
#endif
        /// <summary>
        /// Backing field for <see cref="SQLiteDBPath"/>
        /// </summary>
        private static string? dBPath;

        /// <summary>
        /// Property for the full file path to the SQLite Database
        /// </summary>
        public static string SQLiteDBPath
        {
            get => dBPath ??= GetSQLiteDBPath();
            set => dBPath = value;
        }

        /// <summary>
        /// Method for getting the full file path to SQLite Database
        /// </summary>
        /// <returns>string value of the file path to the SQLite Database</returns>
        public static string GetSQLiteDBPath()
        {
            string p;
#if MIGRATION
            p = Path.Combine("../", SQLiteDbFileName);
#else
            p = Path.Combine(FileSystem.Current.AppDataDirectory, SQLiteDbFileName);
#endif
            return p;
        }
    }
}
