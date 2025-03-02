//#define NEW_DATABASE
//#define SAVECREATESQL

#if (SQLITE)
using Microsoft.Data.Sqlite;
#endif
#if (MSSQL)
using Microsoft.EntityFrameworkCore.SqlServer;
#endif
using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

//#nullable enable 

namespace TimeClockApp.Shared.Models
{
    public partial class DataBackendContext : DbContext
    {
        public static bool Initialized { get; protected set; }
        public static bool FirstRun { get; private set; } = true;
        public static void SetFirstRun(bool IsFirstRun) => FirstRun = IsFirstRun;

        [RequiresUnreferencedCode(
    "EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. "
    + "Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for "
    + "more details.")]
        public DataBackendContext()
        {
            Initialize();
        }
#if MSSQL
        public DataBackendContext(DbContextOptions<DataBackendContext> options) : base(options) {}
#endif

        private void Initialize()
        {
            if (!Initialized)
            {
                try
                {
#if SQLITE
                    SQLitePCL.Batteries_V2.Init();
#endif
#if (NEW_DATABASE && DEBUG)
                    this.Database.EnsureDeleted();
                    this.Database.EnsureCreated();
#endif
#if (SAVECREATESQL && DEBUG)
                    FileService fhs = new();
                    string sqltxt = Path.Combine(fhs.GetDownloadPath(), "CreateDB.sql");

                    string sql = this.Database.GenerateCreateScript();
                    File.WriteAllText(sqltxt, sql);
#endif
#if !NEW_DATABASE
                    Database.Migrate();
#endif
                }
                finally
                {
                    Initialized = true;
                    SetFirstRun(false);
                }
            }
        }

        public DbSet<Employee> Employee { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<TimeCard> TimeCard { get; set; }
        public DbSet<Phase> Phase { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<Expense> Expense { get; set; }
        public DbSet<ExpenseType> ExpenseType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder?
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
#if (DEBUG && !MIGRATION)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging(true)
                    .LogTo(Console.WriteLine, LogLevel.Information)
#else
                    .LogTo(Console.WriteLine, LogLevel.Warning)
#endif
#if SQLITE
                    .UseSqlite($"Filename={SQLiteSetting.GetSQLiteDBPath()}");
#elif MSSQL
                    .UseSqlServer("Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TimeClockApp;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#if MSSQL
#region MSSQLConverters
            modelBuilder.Entity<Employee>()
                .Property(e => e.Employee_PayRate)
                .HasConversion<decimal>();
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasConversion<decimal>();
            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_EmployeePayRate)
                .HasConversion<decimal>();
#endregion
#endif

#region ForeignKeys

            modelBuilder.Entity<TimeCard>()
                .HasOne(t => t.Employee)
                .WithMany(t => t.TimeCards)
                .HasForeignKey(t => t.EmployeeId);

            modelBuilder.Entity<TimeCard>()
                .HasOne(t => t.Project)
                .WithMany(t => t.TimeCards)
                .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<TimeCard>()
                .HasOne(t => t.Phase)
                .WithMany(t => t.TimeCards)
                .HasForeignKey(t => t.PhaseId);

            modelBuilder.Entity<Expense>()
                .HasOne(t => t.Project)
                .WithMany(t => t.Expenses)
                .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<Expense>()
                .HasOne(t => t.Phase)
                .WithMany(t => t.Expenses)
                .HasForeignKey(t => t.PhaseId);

            modelBuilder.Entity<Expense>()
                .HasOne(t => t.ExpenseType)
                .WithMany(t => t.Expenses)
                .HasForeignKey(t => t.ExpenseTypeId);


#endregion ForeignKeys

#region DefaultValues

            modelBuilder.Entity<Employee>()
                .Property(b => b.Employee_Employed)
                .HasDefaultValue(EmploymentStatus.Employed);
            modelBuilder.Entity<TimeCard>()
                .Property(b => b.PhaseId)
                .HasDefaultValue(1);
            modelBuilder.Entity<TimeCard>()
                .Property(b => b.ProjectId)
                .HasDefaultValue(1);
            modelBuilder.Entity<TimeCard>()
                .Property(b => b.TimeCard_DateTime)
                .HasDefaultValueSql("datetime('now', 'localtime')");
            modelBuilder.Entity<Project>()
                .Property(b => b.Status)
                .HasDefaultValue(ProjectStatus.Ready);
            modelBuilder.Entity<Expense>()
                .Property(e => e.IsRecent)
                .HasDefaultValue(true);

#endregion DefaultValues

#region Indexs

            modelBuilder.Entity<TimeCard>()
                 .HasIndex(b => new { b.EmployeeId, b.TimeCard_Status, b.TimeCard_Date, b.TimeCard_bReadOnly})
                 .HasDatabaseName("IX_TimeCardEmpStatDateRead");

            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_WorkHours)
                .HasComputedColumnSql("round((strftime('%s', [TimeCard_EndTime]) - strftime('%s', [TimeCard_StartTime])) / 3600.0, 2)", stored: true);

            modelBuilder.Entity<Employee>()
                .HasIndex(b => new { b.EmployeeId, b.Employee_Employed })
                .HasDatabaseName("IX_EmployeeEmployed");

            modelBuilder.Entity<Project>()
                .HasIndex(p => new { p.ProjectId, p.Status })
                .HasDatabaseName("IX_ProjectStatus");

            modelBuilder.Entity<ExpenseType>()
                .HasIndex(t => new { t.CategoryName })
                .IsUnique();

#endregion Indexs

            modelBuilder.Entity<Config>().HasData(new Config
            {
                ConfigId = 1,
                Name = "Company",
                StringValue = "Alexander Builder",
                Hint = "The business entity name"
            },
            new Config
            {
                ConfigId = 2,
                Name = "FirstRun",
                StringValue = DateOnly.FromDateTime(DateTime.Now).ToShortDateString(),
                Hint = "Date this app was 1st ran. For internal use only"
            },
            new Config
            {
                ConfigId = 3,
                Name = "CurrentProject",
                IntValue = 1,
                Hint = "Current Project to default to"
            },
            new Config
            {
                ConfigId = 4,
                Name = "CurrentPhase",
                IntValue = 1,
                Hint = "Current Phase to default to"
            },
            new Config
            {
                ConfigId = 5,
                Name = "WorkCompRate",
                StringValue = "0.171118",
                Hint = "Worker Comp Rate per $100 remuneration"
            },
            new Config
            {
                ConfigId = 6,
                Name = "EstimateEmployerTaxes",
                StringValue = "0.1019"
            },
            new Config
            {
                ConfigId = 7,
                Name = "ProfitRate",
                StringValue = "0.1"
            },
            new Config
            {
                ConfigId = 8,
                Name = "OverHeadRate",
                StringValue = "0.08"
            },
            new Config
            {
                ConfigId = 9,
                Name = "IsAdmin",
                IntValue = 0,
                Hint = "Enables dangerous timecard edits"
            },
            new Config
            {
                ConfigId = 10,
                Name = "DaysInPayPeriod",
                IntValue = 7,
                Hint = "Number of Days in a Pay Period (weekly=7,biweekly=14,etc) (Default 7)"
            },
            new Config
            {
                ConfigId = 11,
                Name = "PayDayOfWeek",
                IntValue = 5,
                Hint = "Day of week that is the end of the pay period (0=Sunday...3=Wednesday...5=Friday,6=Saturday)(Default 5)"
            },
            new Config
            {
                ConfigId = 12,
                Name = "Version",
                StringValue = "1.7",
                Hint = "Application Database version"
            });

            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeId = 1,
                Employee_Name = "John Doe",
                Employee_PayRate = 20.00,
                JobTitle = "Job Title",
                Employee_Employed = EmploymentStatus.Employed
            },
            new Employee
            {
                EmployeeId = 2,
                Employee_Name = "Jane Doe",
                Employee_PayRate = 25.00,
                JobTitle = "Job Title",
                Employee_Employed = EmploymentStatus.Employed
            });

            modelBuilder.Entity<ExpenseType>().HasData(
            new ExpenseType
            {
                ExpenseTypeId = 1,
                CategoryName = "Deleted"
            },
            new ExpenseType
            {
                ExpenseTypeId = 2,
                CategoryName = "Income"
            },
            new ExpenseType
            {
                ExpenseTypeId = 3,
                CategoryName = "Payroll"
            },
            new ExpenseType
            {
                ExpenseTypeId = 4,
                CategoryName = "WorkersComp"
            },
            new ExpenseType
            {
                ExpenseTypeId = 5,
                CategoryName = "Materials"
            },
            new ExpenseType
            {
                ExpenseTypeId = 6,
                CategoryName = "Toll.Gas"
            },
            new ExpenseType
            {
                ExpenseTypeId = 7,
                CategoryName = "Misc"
            },
            new ExpenseType
            {
                ExpenseTypeId = 8,
                CategoryName = "Refund"
            },
            new ExpenseType
            {
                ExpenseTypeId = 9,
                CategoryName = "Subcontractor"
            },
            new ExpenseType
            {
                ExpenseTypeId = 10,
                CategoryName = "Taxes"
            },
            new ExpenseType
            {
                ExpenseTypeId = 11,
                CategoryName = "Dumps"
            },
            new ExpenseType
            {
                ExpenseTypeId = 12,
                CategoryName = "Overhead"
            },
            new ExpenseType
            {
                ExpenseTypeId = 13,
                CategoryName = "Permit"
            });

            string[] projectNames = new string[] { ".None", "Sample" };
            DateOnly date = DateOnly.FromDateTime(DateTime.Now);

            for (int i = 0; i < projectNames.Length; i++)
            {
                modelBuilder.Entity<Project>().HasData(new Project
                {
                    ProjectId = i + 1,
                    Name = projectNames[i],
                    Status = ProjectStatus.Active,
                    ProjectDate = date
                });
            }

            string[] phaseTitles = new string[] { ".Misc", "Phase 1", "Phase 2", "Phase 3", "Office Work" };

            for (int i = 0; i < phaseTitles.Length; i++)
            {
                modelBuilder.Entity<Phase>().HasData(new Phase
                {
                    PhaseId = i + 1,
                    PhaseTitle = phaseTitles[i]
                });
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
