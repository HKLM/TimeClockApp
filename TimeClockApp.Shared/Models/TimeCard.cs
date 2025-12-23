using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

namespace TimeClockApp.Shared.Models
{
    /// <summary>ShiftStatus enum
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <item>
    /// <term>0</term>
    /// <description>
    /// NA (No Record)
    /// </description>
    /// </item>
    /// <item>
    /// <term>1</term>
    /// <description>
    /// ClockedIn (Employee is currently on the clock)
    /// </description>
    /// </item>
    /// <item>
    /// <term>2</term>
    /// <description>
    /// ClockedOut (Employee is no longer on the clock. TimeCard to be processed in next payroll)
    /// </description>
    /// </item>
    /// <item>
    /// <term>3</term>
    /// <description>
    /// Paid (TimeCard has been paid to Employee)
    /// </description>
    /// </item>
    /// <item>
    /// <term>4</term>
    /// <description>
    /// Deleted (TimeCard Marked as void)
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum ShiftStatus
    {
        [Description("No Record")]
        NA = 0,
        [Description("Clocked In")]
        ClockedIn = 1,
        [Description("Clocked Out")]
        ClockedOut = 2,
        [Description("Paid")]
        Paid = 3,
        [Description("Deleted")]
        Deleted = 4
    }

    /// <summary>
    /// Record of single continuous amount of time, the Employee worked in a single day.
    /// </summary>
    /// <remarks>
    /// TODO -If breaks are not paid time, end current TimeCard at the beginning of break.
    /// Start a new TimeCard upon end of the break time or add a breaktime column
    /// </remarks>
    public class TimeCard
    {
        public TimeCard() { }

        public TimeCard(Employee Employee)
        {
            ArgumentNullException.ThrowIfNull(Employee);
            EmployeeId = Employee.EmployeeId;
            TimeCard_EmployeeName = Employee.Employee_Name ?? throw new ArgumentNullException(nameof(Employee.Employee_Name));
            TimeCard_EmployeePayRate = Employee.Employee_PayRate;
            this.Employee = Employee ?? throw new ArgumentNullException(nameof(Employee));
            TimeCard_DateTime = DateTime.Now;
            TimeCard_Date = DateOnly.FromDateTime(DateTime.Now);
            TimeCard_Status = ShiftStatus.NA;
            ProjectId = 1;
            ProjectName = ".None";
            PhaseId = 1;
            PhaseTitle = ".Misc";
            TimeCard_bReadOnly = false;
        }

        public TimeCard(Employee Employee, int ProjectId, int PhaseId, string ProjectName, string PhaseTitle, TimeOnly TimeCard_StartTime)
        {
            ArgumentNullException.ThrowIfNull(Employee);
            EmployeeId = Employee.EmployeeId;
            TimeCard_EmployeeName = Employee.Employee_Name ?? throw new ArgumentNullException(nameof(Employee.Employee_Name));
            TimeCard_EmployeePayRate = Employee.Employee_PayRate;
            this.Employee = Employee ?? throw new ArgumentNullException(nameof(Employee));
            TimeCard_DateTime = DateTime.Now;
            TimeCard_Date = DateOnly.FromDateTime(DateTime.Now);
            this.TimeCard_StartTime = TimeCard_StartTime;
            TimeCard_Status = ShiftStatus.ClockedIn;
            this.ProjectId = ProjectId;
            this.ProjectName = string.IsNullOrEmpty(ProjectName) ? ".None" : ProjectName;
            this.PhaseId = PhaseId;
            this.PhaseTitle = string.IsNullOrEmpty(PhaseTitle) ? ".Misc" : PhaseTitle;
            TimeCard_bReadOnly = false;
        }

        [Key]
        public int TimeCardId { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public int PhaseId { get; set; }

        /// <summary>
        /// Copied from the associated Employee entity at the time this TimeCard is created.
        /// </summary>
        [Required]
        public string TimeCard_EmployeeName { get; set; } = string.Empty;

        /// <summary>ShiftStatus enum
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>
        /// <term>0</term>
        /// <description>
        /// NA (No Record)
        /// </description>
        /// </item>
        /// <item>
        /// <term>1</term>
        /// <description>
        /// ClockedIn (Employee is currently on the clock)
        /// </description>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <description>
        /// ClockedOut (Employee is no longer on the clock. TimeCard to be processed in next payroll)
        /// </description>
        /// </item>
        /// <item>
        /// <term>3</term>
        /// <description>
        /// Paid (TimeCard has been paid to Employee)
        /// </description>
        /// </item>
        /// <item>
        /// <term>4</term>
        /// <description>
        /// Deleted (TimeCard Marked as void)
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        [Required]
        public ShiftStatus TimeCard_Status { get; set; }

        /// <summary>
        /// DateTime stamp of when this TimeCard begins
        /// </summary>
        /// <remarks>
        /// Used in query sorting operations.
        /// </remarks>
        public DateTime TimeCard_DateTime { get; set; }

        /// <summary>
        /// Date of this TimeCard.
        /// </summary>
        /// <remarks>
        /// Used for display in UI and quicker queries when looking for a Date
        /// </remarks>
        public DateOnly TimeCard_Date { get; set; }

        /// <summary>
        /// Time Employee begins their shift
        /// </summary>
        public TimeOnly TimeCard_StartTime { get; set; }

        /// <summary>
        /// Time the Employee clocks out of their shift.
        /// </summary>
        /// <remarks>
        /// Time entry must be for the same day. If working up to and past midnight 12AM,
        /// set the EndTime of this TimeCard to 11:59PM and begin a new TimeCard starting at 12AM
        /// </remarks>
        public TimeOnly TimeCard_EndTime { get; set; }

        [NotMapped]
        public TimeSpan TimeCard_Duration
        {
            get
            {
                if (TimeCard_Status == ShiftStatus.NA || TimeCard_Status == ShiftStatus.Deleted)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    if (TimeCard_EndTime == new TimeOnly(0, 0))
                    {
                        TimeOnly o = TimeOnly.FromDateTime(DateTime.Now);
                        return o - TimeCard_StartTime;
                    }
                    return TimeCard_EndTime - TimeCard_StartTime;
                }
            }
        }

        /// <summary>
        /// SQL computed column of the amount of time worked for this timecard
        /// </summary>
        public double TimeCard_WorkHours { get; set; }

        /// <summary>
        /// Used on TimeCardPage to display the total hours clocked (from all TimeCards in the current pay period)
        /// </summary>
        [NotMapped]
        public double TotalWorkHours { get; set; }

        /// <summary>
        /// The Employee's payrate, at the time of this TimeCard
        /// </summary>
        [Required]
        public double TimeCard_EmployeePayRate { get; set; }

        /// <summary>
        /// When set to TRUE, Permanently prevents all further changes to this TimeCard.
        /// </summary>
        [Required]
        public bool TimeCard_bReadOnly { get; set; }

        /// <summary>
        /// Saved Project Name at the time this card was made.
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Saved Project Title at the time this card was made.
        /// </summary>
        public string PhaseTitle { get; set; } = string.Empty;

        //Navigation Entities
        public virtual Employee Employee { get; set; }
        public virtual Project Project { get; set; }
        public virtual Phase Phase { get; set; }
    }

    public sealed class TimeCardMap : ClassMap<TimeCard>
    {
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        public TimeCardMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.TimeCardId);
            Map(m => m.EmployeeId);
            Map(m => m.ProjectId);
            Map(m => m.PhaseId);
            Map(m => m.TimeCard_EmployeeName);
            Map(m => m.TimeCard_Status);
            Map(m => m.TimeCard_DateTime);
            Map(m => m.TimeCard_Date);
            Map(m => m.TimeCard_StartTime);
            Map(m => m.TimeCard_EndTime);
            Map(m => m.TimeCard_WorkHours);
            Map(m => m.TimeCard_EmployeePayRate);
            Map(m => m.TimeCard_bReadOnly);
            Map(m => m.ProjectName).Optional();
            Map(m => m.PhaseTitle).Optional();
        }
    }
}
