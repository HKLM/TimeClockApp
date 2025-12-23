#define MIGRATION

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TimeClockApp.Shared.Models;

namespace EFMigrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting DB Migration...");
            using (var db = new DataBackendContext())
            {
                var all = db.Employee.ToList();
            }
        }

        public class ContextFactory : IDesignTimeDbContextFactory<DataBackendContext>
        {
            public DataBackendContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<DataBackendContext>();
#if MSSQL
                optionsBuilder.UseSqlServer($"Data Source={TimeClockApp.Shared.MSSQLSetting.GetMSSQLconnectionString()}");
#else
                optionsBuilder.UseSqlite($"Filename={TimeClockApp.Shared.SQLiteSetting.GetSQLiteDBPath()}");
#endif

                return new DataBackendContext(optionsBuilder.Options);
            }
        }
    }
}
