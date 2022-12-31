#define USESQLITE
//#define TRIGGER
//#define TIMESTAMP

//#define NEW_DATABASE
//#define SAVECREATESQL

#if TRIGGER
using Microsoft.Data.Sqlite;
#endif
using Microsoft.EntityFrameworkCore;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace TimeClockApp.Models
{
    public partial class DatabackendContext : DbContext
    {
        public DatabackendContext()
        {
            if (App.FirstRun)
            {
#if NEW_DATABASE
                this.Database.EnsureDeleted();
#endif
#if USESQLITE
                if (this.Database.EnsureCreated())
#else
                if (File.Exists(App.SQLiteDBPath))
                    this.Database.Migrate();
                else
#endif
                    CreateDBTrigger();
#if SAVECREATESQL
                FileHelperService fhs = new();
                string sqltxt = fhs.GetDBPath("CreateDB.sql");
                string sql = this.Database.GenerateCreateScript();
                File.WriteAllText(sqltxt, sql);
#endif
                App.SetFirstRun(false);
            }
        }

        private int CreateDBTrigger()
        {
            int i = 0;
            if (this.Config != null)
            {
                int z = 2;
                Config c = this.Config.Find(z);
                if (c.StringValue == null || c.StringValue == string.Empty)
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
        public DbSet<Wages> Wages { get; set; }
        public DbSet<Expense> Expense { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder != null)
            {
                optionsBuilder
#if DEBUG
                    .EnableSensitiveDataLogging(true)
                    .EnableDetailedErrors()
                    .LogTo(Console.WriteLine, LogLevel.Debug)
#endif
                    .UseSqlite($"Filename={App.SQLiteDBPath}");
            }
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
                .WithMany(e => e.TimeCards)
                .HasForeignKey(t => t.EmployeeId);

            modelBuilder.Entity<TimeCard>()
                .HasOne(t => t.Project)
                .WithMany(e => e.TimeCards)
                .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<TimeCard>()
                .HasOne(t => t.Phase)
                .WithMany(e => e.TimeCards)
                .HasForeignKey(t => t.PhaseId);

            modelBuilder.Entity<Wages>()
                .HasOne(t => t.TimeCard)
                .WithOne(e => e.Wages)
                .HasForeignKey<TimeCard>(t => t.WagesId);

            modelBuilder.Entity<Expense>()
                .HasOne(t => t.Project)
                .WithMany(e => e.Expenses)
                .HasForeignKey(t => t.ProjectId);

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
                 .HasIndex(b => b.TimeCard_DateTime)
                 .IsDescending(new bool[] { true })
                 .HasDatabaseName("IX_TimeCardDateTime");

            modelBuilder.Entity<TimeCard>()
                .Property(t => t.TimeCard_WorkHours)
                .HasComputedColumnSql("round((strftime('%s', [TimeCard_EndTime]) - strftime('%s', [TimeCard_StartTime])) / 3600.0, 2)", stored: true);

            #endregion Indexs


            modelBuilder.Entity<Config>().HasData(new Config
            {
                ConfigId = 1,
                Name = "Company",
                StringValue = "TimeClock App",
                Hint = "The businuess entity name"
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
                Name = "EstimateEmployerTaxes", /* TODO */
                StringValue = "0.1019073159256645"
            },
            new Config
            {
                ConfigId = 7,
                Name = "ProfitRate", /* TODO */
                IntValue = 10
            },
            new Config
            {
                ConfigId = 8,
                Name = "OverHeadRate", /* TODO */
                IntValue = 8
            },
            new Config
            {
                ConfigId = 9,
                Name = "IsAdmin",
                IntValue = 0,
                Hint = "Enables dangerous timecard edits"
            });

            // Default employees that should be created upon the database creation
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                EmployeeId = 1,
                Employee_Name = "Employee 1",
                Employee_PayRate = 20.00,
                JobTitle = "Technician",
                Employee_Employed = EmploymentStatus.Employed
            }
            );

            DateOnly date = DateOnly.FromDateTime(DateTime.Now);

            // Default project names that should be created upon the database creation
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

            // Default phase titles that should be created upon the database creation
            string[] phaseTitles = new string[] {".Misc","Cement","Cement-Forms","Framing","Prep-Painting","Painting","Bathroom","Deck","Demo",
                "Doors/Windows","Drywall","Electrical","Fence","Finish Hardware","Flooring","HVAC","Insulation","Irrigation","Kitchen","Landscaping",
                "Plumbing","Siding","Stucco","Stucco-Lath","Subfloor","Tile","Trim/Baseboards","Building Paper","Drywall-Tape+Mud",
                "Stairs","Data/Comm/AV","Countertop","Excavation","Administrative","Clean Up","Roofing","Masonary/Brick","Dumps","Cabinets",
                "Light Fixtures","Water Heater","Inspection"};

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
            return DateTime.UtcNow;
        }

        private void OnBeforeSaving()
        {
            IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries = ChangeTracker.Entries();
            DateTime utcNow = GetUTC();

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
