#nullable enable

namespace TimeClockApp.Shared
{
    /// <summary>
    /// Class holds the MSSQL Database connection string
    /// </summary>
    public static class MSSQLSetting
    {
        //TODO : Make this user configurable and not hard coded.
#if !MSSQL
        /// <summary>
        /// No MSSQL connection string defined
        /// </summary>
        private const string mSQLconnStr = "";
#else
        /// <summary>
        /// MSSQL Database connection string
        /// </summary>
        /// <remarks>Include everything after Data Source=</remarks>
        private const string mSQLconnStr = "(localdb)\\MSSQLLocalDB;Initial Catalog=TimeClockApp;Persist Security Info=True;User ID=username;Password=password;Encrypt=True;Trust Server Certificate=True;Authentication=SqlPassword;";
#endif
        private static string MSSQLconnStr { get; } = mSQLconnStr;

        /// <summary>
        /// Method for getting the MSSQL Database connection string
        /// </summary>
        /// <returns>string value of the connection string to the MSSQL Database</returns>
        public static string GetMSSQLconnectionString() => MSSQLconnStr;
    }
}
