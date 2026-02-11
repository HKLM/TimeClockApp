//#define NEW_DATABASE
#if SQLITE && MSSQL
#error Cannot define both SQLITE and MSSQL
#endif
#if MSSQL
using Microsoft.EntityFrameworkCore.SqlServer;
#else
using Microsoft.Data.Sqlite;
#endif
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

#nullable enable 

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

        [RequiresUnreferencedCode(
"EF Core isn't fully compatible with trimming, and running the application may generate unexpected runtime failures. "
+ "Some specific coding pattern are usually required to make trimming work properly, see https://aka.ms/efcore-docs-trimming for "
+ "more details.")]
        public DataBackendContext(DbContextOptions<DataBackendContext> options) : base(options) 
        {
#if MSSQL
            Database.Migrate();
            Thread.Sleep(3000);
#else
            SQLitePCL.Batteries_V2.Init();
            Database.Migrate();
#endif
        }

        private void Initialize()
        {
            if (!Initialized)
            {
                try
                {
#if !MSSQL
                    SQLitePCL.Batteries_V2.Init();
#endif
#if NEW_DATABASE
                    this.Database.EnsureDeleted();
                    Thread.Sleep(3000);
                    this.Database.EnsureCreated();
                    Thread.Sleep(3000);
#endif

                    Database.Migrate();
#if MSSQL
                    Thread.Sleep(3000);
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
#if DEBUG
                    .EnableDetailedErrors(true)
                    .EnableSensitiveDataLogging(true)
                    .LogTo(message => System.Diagnostics.Debug.WriteLine(message), LogLevel.Information)
#else
                    .LogTo(message => Console.WriteLine(message), LogLevel.Warning)
#endif
#if MSSQL
                    .UseSqlServer($"Data Source={TimeClockApp.Shared.MSSQLSetting.GetMSSQLconnectionString()}");
#else
                    .UseSqlite($"Filename={SQLiteSetting.GetSQLiteDBPath()}");
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#region MSSQLConverters
#if MSSQL
            modelBuilder.Entity<Employee>()
                .Property(e => e.Employee_PayRate)
                .HasColumnType("decimal(8,2)")
                .HasConversion<decimal>();
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .IsRequired(required: true)
                .HasColumnType("decimal(8,2)")
                .HasConversion<decimal>();
            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_EmployeePayRate)
                .HasColumnType("decimal(8,2)")
                .HasConversion<decimal>();
            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_WorkHours)
                .HasColumnType("decimal(4,2)")
                .HasConversion<decimal>();
            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_EmployeePayRate)
                .HasColumnType("decimal(8,2)")
                .HasConversion<decimal>();
#endif
#endregion

            modelBuilder.Entity<Employee>()
                .Property(t => t.Employee_Name)
                .HasMaxLength(50);
            modelBuilder.Entity<Expense>()
                .Property(t => t.ExpenseDate)
                .HasColumnType("date");
            modelBuilder.Entity<ExpenseType>()
                .Property(t => t.CategoryName)
                .HasMaxLength(50)
                .IsRequired(true);
            modelBuilder.Entity<TimeCard>(
                e =>
                {
                    e.Property(t => t.TimeCard_DateTime).IsRequired(true);
                    e.Property(t => t.TimeCard_Date).HasColumnType("date").IsRequired(true);
                    e.Property(t => t.TimeCard_StartTime).HasColumnType("time").HasPrecision(0);
                    e.Property(t => t.TimeCard_EndTime).HasColumnType("time").HasPrecision(0);
                    e.Property(t => t.TimeCard_EmployeeName).HasMaxLength(50);
                    e.Property(t => t.ProjectName).HasMaxLength(50);
                    e.Property(t => t.PhaseTitle).HasMaxLength(50);
                });
            modelBuilder.Entity<Project>()
                .Property(t => t.ProjectDate)
                .HasColumnType("date");

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
#if MSSQL
            modelBuilder.Entity<TimeCard>()
                .Property(b => b.TimeCard_DateTime)
                .HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_WorkHours)
                .HasComputedColumnSql("CONVERT([decimal](4,2),datediff(minute,[TimeCard_StartTime],[TimeCard_EndTime])/(60.0))", stored: true);
#else
            modelBuilder.Entity<TimeCard>()
                .Property(b => b.TimeCard_DateTime)
                .HasDefaultValueSql("datetime('now', 'localtime')");
            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_WorkHours)
                .HasComputedColumnSql("round((strftime('%s', [TimeCard_EndTime]) - strftime('%s', [TimeCard_StartTime])) / 3600.0, 2)", stored: true);
#endif
            modelBuilder.Entity<Project>()
                .Property(b => b.Status)
                .HasDefaultValue(ProjectStatus.Ready);
            modelBuilder.Entity<Expense>()
                .Property(e => e.IsRecent)
                .HasDefaultValue(true);

#endregion DefaultValues

#region Indexs

            modelBuilder.Entity<Employee>()
                .HasIndex(b => new { b.Employee_Name})
                .IsUnique(true);

            modelBuilder.Entity<ExpenseType>()
                .HasIndex(t => new { t.CategoryName })
                .IsUnique(true);

            modelBuilder.Entity<Phase>()
                .HasIndex(t => new { t.PhaseTitle })
                .IsUnique(true);

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
                StringValue = "1.8",
                Hint = "Application Database version"
            },
            new Config
            {
                ConfigId = 13,
                Name = "AppTheme",
                IntValue = 0,
                Hint = "Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)"
            });

            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeId = 1,
                Employee_Name = "John Doe",
                Employee_PayRate = 25.00,
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
            new ExpenseType { ExpenseTypeId = 1, CategoryName = "Deleted" },
            new ExpenseType { ExpenseTypeId = 2, CategoryName = "Income" },
            new ExpenseType { ExpenseTypeId = 3, CategoryName = "Payroll" },
            new ExpenseType { ExpenseTypeId = 4, CategoryName = "WorkersComp" },
            new ExpenseType { ExpenseTypeId = 5, CategoryName = "Materials" },
            new ExpenseType { ExpenseTypeId = 6, CategoryName = "Toll.Gas" },
            new ExpenseType { ExpenseTypeId = 7, CategoryName = "Misc" },
            new ExpenseType { ExpenseTypeId = 8, CategoryName = "Refund" },
            new ExpenseType { ExpenseTypeId = 9, CategoryName = "Subcontractor" },
            new ExpenseType { ExpenseTypeId = 10, CategoryName = "Taxes" },
            new ExpenseType { ExpenseTypeId = 11, CategoryName = "Dumps" },
            new ExpenseType { ExpenseTypeId = 12, CategoryName = "Overhead" },
            new ExpenseType { ExpenseTypeId = 13, CategoryName = "Permit" });

            DateOnly date = DateOnly.FromDateTime(DateTime.Now);
            string[] projectNames = new string[] { ".None", "Sample" };

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

            string[] phaseTitles = new string[] {".Misc","Cement","Cement-Forms","Framing","Prep-Painting","Painting","Bathroom","Crown Molding","Deck","Demo",
                            "Doors","Drywall","Electric-Finish","Electrical","Fence","Finish Hardware","Flooring","HVAC","Insulation","Irrigation","Kitchen","Landscaping",
                            "Plumbing","Siding","Stucco","Stucco-Lath","Subfloor","Drywall-Texture","Tile","Trim/Baseboards","Windows","Building Paper","Drywall-Tape+Mud",
                            "Stairs","Data/Comm/AV","Countertop","Excavation","Administrative","Clean Up","Roofing","Masonry/Brick","Dumps","Cabinets","Moving","Gas Line",
                            "Light Fixtures","Plumbing-Rough","Water Heater","Blueprints","Inspection"};
            Array.Sort(phaseTitles);

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
