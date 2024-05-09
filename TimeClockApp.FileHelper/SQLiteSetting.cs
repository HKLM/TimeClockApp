//#define MIGRATION
using Microsoft.Maui.Storage;

using System.IO;

namespace TimeClockApp.FileHelper
{
    public static class SQLiteSetting
    {
#if MIGRATION
        /// <summary>
        /// File name (only) of the SQLite Database used by the EFMigrator for setting up EF migrations.
        /// </summary>
        /// <remarks>Does not include path.</remarks>
        public const string SQLiteDbFileName = "BaseTimeClock.db3";
#else
        /// <summary>
        /// File name (only) of the SQLite Database used by the app.
        /// </summary>
        /// <remarks>Does not include path.</remarks>
        public const string SQLiteDbFileName = "TimeClockAppDB-04.db3";
#endif
        private static string dBPath;

        /// <summary>
        /// Get the full file path to SQLite Database
        /// </summary>
        public static string SQLiteDBPath
        {
            get => dBPath ??= GetSQLiteDBPath();
            set => dBPath = value;
        }

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
