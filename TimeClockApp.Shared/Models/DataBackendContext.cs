//#define TRIGGER
//#define TIMESTAMP
//#define NEW_DATABASE
//#define SAVECREATESQL

#if TRIGGER
using Microsoft.Data.Sqlite;
#endif
using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using TimeClockApp.FileHelper;

namespace TimeClockApp.Shared.Models
{
    [RequiresUnreferencedCode("Calls DynamicBehavior.")]
    public partial class DataBackendContext : DbContext
    {
        public static bool Initialized { get; protected set; }

        public static bool FirstRun { get; private set; } = true;
        public static void SetFirstRun(bool IsFirstRun) => FirstRun = IsFirstRun;

        public DataBackendContext()
        {
#if MIGRATION
            SQLiteSetting.SQLiteDBPath = Path.Combine("../", "BaseTimeClock.db3");
#else
            SQLiteSetting.SQLiteDBPath = SQLiteSetting.GetSQLiteDBPath();
#endif
            Initialize();
        }

        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        public DataBackendContext(string _DbFilePath)
        {
            SQLiteSetting.SQLiteDBPath = _DbFilePath;
            Initialize();
        }

        private void Initialize()
        {
            if (!Initialized)
            {
                try
                {
#if NEW_DATABASE
                    this.Database.EnsureDeleted();
#endif
#if SAVECREATESQL
                    FileService fhs = new();
                    string sqltxt = Path.Combine(fhs.GetDownloadPath(), "CreateDB.sql");
                    //string sqltxt = fhs.GetDBPath("CreateDB.sql");

                    string sql = this.Database.GenerateCreateScript();
                    File.WriteAllText(sqltxt, sql);
#endif
                    SQLitePCL.Batteries_V2.Init();
                    Database.Migrate();
                    CreateDBTrigger();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException, "DB");
                    //throw;
                }
                finally
                {
                    Initialized = true;
                    SetFirstRun(false);
                }
            }
        }

        private int CreateDBTrigger()
        {
            int i = 0;
            if (this.Config != null)
            {
                const int z = 2;
                Config c = this.Config.Find(z);
                if (string.IsNullOrEmpty(c.StringValue))
                {
                    c.StringValue = DateOnly.FromDateTime(DateTime.Now).ToShortDateString();
                    this.Config.Update(c);

                    i = this.SaveChanges();
                }
            }
#if TRIGGER
            if (this.Database != null)
            {
                try
                {
                    string s = "CREATE TRIGGER IF NOT EXISTS TimeCardReadOnlyTrigger \r\n"
                            + "BEFORE UPDATE \r\n ON TimeCard \r\n BEGIN \r\n"
                            + "    SELECT CASE WHEN OLD.TimeCard_bReadOnly == 1 THEN RAISE(ABORT, \"Access denied. Item is ReadOnly\") \r\n END; \r\n END; \r\n";
                    i = this.Database.ExecuteSqlRaw(s);

                }
                catch (SqliteException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message + "\n" + ex.InnerException);
                    App.AlertSvc.ShowAlert("SQLiteException", "Error in creating Database trigger.\n" + ex.Message + "\n" + ex.InnerException);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.InnerException);
                    App.AlertSvc.ShowAlert("Exception", "Error in creating Database trigger.\n" + e.Message + "\n" + e.InnerException);
                }
            }
#endif
            return i;
        }

        public DbSet<Employee> Employee { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<TimeCard> TimeCard { get; set; }
        public DbSet<Phase> Phase { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<Expense> Expense { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder?
#if DEBUG
                    .EnableSensitiveDataLogging(true)
                    .EnableDetailedErrors()
#endif
#if TRACE && DEBUG
                    .LogTo(Console.WriteLine, LogLevel.Debug)
#endif
                    .UseSqlite($"Filename={SQLiteSetting.SQLiteDBPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#if TIMESTAMP
            if (modelBuilder is null)
            {
                OnModelCreatingPartial(modelBuilder);
                return;
            }
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

            modelBuilder.Entity<TimeSheet>()
                .HasOne(t => t.Employee)
                .WithMany(t => t.TimeSheets)
                .HasForeignKey(t => t.EmployeeId)
                .IsRequired(false);

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

            modelBuilder.Entity<TimeSheet>()
                 .HasIndex(b => new { b.EmployeeId, b.Status, b.PayPeriodWeekNum })
                 .HasDatabaseName("IX_TimeSheetEmpStatWeek");

#endregion Indexs

            modelBuilder.Entity<Config>().HasData(new Config
            {
                ConfigId = 1,
                Name = "Company",
                StringValue = "Company Name",
                Hint = "The business entity name"
            },
            new Config
            {
                ConfigId = 2,
                Name = "FirstRun",
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
                IntValue = 21,
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
                IntValue = 10
            },
            new Config
            {
                ConfigId = 8,
                Name = "OverHeadRate",
                IntValue = 8
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
                Hint = "Number of Days in a Pay Period (7,14,etc)"
            },
            new Config
            {
                ConfigId = 11,
                Name = "PayDayOfWeek",
                IntValue = 5,
                Hint = "Day of week that is the end of the pay period (0=Sunday...3=Wednesday...5=Friday,6=Saturday)"
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

            DateOnly date = DateOnly.FromDateTime(DateTime.Now);
            string[] projectNames = new string[] {".None","Sample" };
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
#if TIMESTAMP
            OnModelCreatingPartial(modelBuilder);
#endif
        }

#if TIMESTAMP

        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


        /// <summary>
        /// Gets the current UTC DateTime of DateTimeKind.Utc
        /// </summary>
        /// <returns>DateTime.UtcNow of DateTimeKind.Utc</returns>
        public static DateTime GetUTC()
        {
            return DateTime.Now;
        }

        private void OnBeforeSaving()
        {
            IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries = ChangeTracker.Entries();
            DateTime utcNow = DateTime.Now;

            foreach (var entry in entries)
            {
                // for entities that inherit from BaseEntity, set UpdatedOn / CreatedOn appropriately
                if (entry.Entity is BaseEntity trackable)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            // set the updated date to "now"
                            trackable.DateModified = utcNow;
                            // mark property as "don't touch" we don't want to update on a Modify operation
                            entry.Property("DateCreated").IsModified = false;
                            break;

                        case EntityState.Added:
                            // set both updated and created date to "now"
                            trackable.DateCreated = utcNow;
                            trackable.DateModified = utcNow;
                            break;
                    }
                }
            }
        }
#endif
    }
}
