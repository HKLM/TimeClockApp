//#define TIMESTAMP
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    /// Paid (TimeCard has been paid to employee)
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
    /// Record of single continuous amount of time, the employee worked in a single day.
    /// </summary>
    /// <remarks>
    /// TODO -If breaks are not paid time, end current TimeCard at the beginning of break.
    /// Start a new TimeCard upon end of the break time or add a breaktime column
    /// </remarks>
#if TIMESTAMP
    public class TimeCard : BaseEntity
#else
    public class TimeCard
#endif
    {
        public TimeCard() { }

        public TimeCard(Employee employee)
        {
            ArgumentNullException.ThrowIfNull(employee);
            EmployeeId = employee.EmployeeId;
            TimeCard_EmployeeName = employee.Employee_Name ?? throw new ArgumentNullException(nameof(employee.Employee_Name));
            TimeCard_EmployeePayRate = employee.Employee_PayRate;
            Employee = employee ?? throw new ArgumentNullException(nameof(employee));
            TimeCard_DateTime = DateTime.Now;
            TimeCard_Date = DateOnly.FromDateTime(DateTime.Now);
            TimeCard_Status = ShiftStatus.NA;
            ProjectId = 1;
            ProjectName = ".None";
            PhaseId = 1;
            PhaseTitle = ".Misc";
            TimeCard_bReadOnly = false;
        }

        public TimeCard(Employee employee, int projectId, int phaseId)
        {
            ArgumentNullException.ThrowIfNull(employee);
            EmployeeId = employee.EmployeeId;
            TimeCard_EmployeeName = employee.Employee_Name ?? throw new ArgumentNullException(nameof(employee.Employee_Name));
            TimeCard_EmployeePayRate = employee.Employee_PayRate;
            Employee = employee ?? throw new ArgumentNullException(nameof(employee));
            TimeCard_DateTime = DateTime.Now;
            TimeCard_Date = DateOnly.FromDateTime(DateTime.Now);
            TimeCard_Status = ShiftStatus.NA;
            ProjectId = projectId;
            //TODDO
            ProjectName = ".None";
            //ProjectName = EditProjectService.GetProjectNameFromId(projectId);
            PhaseId = phaseId;
            //TODDO
            PhaseTitle = ".Misc";
            //PhaseTitle = EditPhaseService.GetPhaseTitleFromId(phaseId);
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
        public string TimeCard_EmployeeName { get; set; }

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
        /// Paid (TimeCard has been paid to employee)
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
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime TimeCard_DateTime { get; set; }

        /// <summary>
        /// Date of this TimeCard.
        /// </summary>
        /// <remarks>
        /// Used for display in UI and quicker queries when looking for a Date
        /// </remarks>
        [Required]
        [Column(TypeName = "date")]
        public DateOnly TimeCard_Date { get; set; }

        /// <summary>
        /// Time employee begins their shift
        /// </summary>
        [Column(TypeName = "time")]
        public TimeOnly TimeCard_StartTime { get; set; }

        /// <summary>
        /// Time the employee clocks out of their shift.
        /// </summary>
        /// <remarks>
        /// Time entry must be for the same day. If working up to and past midnight 12AM,
        /// set the EndTime of this TimeCard to 11:59PM and begin a new TimeCard starting at 12AM
        /// </remarks>
        [Column(TypeName = "time")]
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
        [Column(TypeName = "double")]
        public double TimeCard_WorkHours { get; set; }

        /// <summary>
        /// Used on TimeCardPage to display the total hours clocked (from all TimeCards in the current pay period)
        /// </summary>
        [NotMapped]
        [Column(TypeName = "double")]
        public double TotalWorkHours { get; set; }

        /// <summary>
        /// The employee's payrate, at the time of this TimeCard
        /// </summary>
        [Required]
        [Column(TypeName = "double")]
        public double TimeCard_EmployeePayRate { get; set; }

        /// <summary>
        /// When set to TRUE, Permanently prevents all further changes to this TimeCard.
        /// </summary>
        [Required]
        public bool TimeCard_bReadOnly { get; set; }

        /// <summary>
        /// Saved Project Name at the time this card was made.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Saved Project Title at the time this card was made.
        /// </summary>
        public string PhaseTitle { get; set; }

        //Navigation Entities
        public virtual Employee Employee { get; set; }
        public virtual Project Project { get; set; }
        public virtual Phase Phase { get; set; }

#if DEBUG
        public override string ToString()
        {
            string rv = "\n--------------[  TimeCardId: " + TimeCardId + "  ]---------------------\n";
            rv += "EmployeeId:  " + EmployeeId + Environment.NewLine;
            rv += "EmployeeName:" + TimeCard_EmployeeName + Environment.NewLine;
            rv += "Status:      " + TimeCard_Status.ToString() + Environment.NewLine;
            rv += "DateTime:    " + TimeCard_DateTime.ToShortDateString() + Environment.NewLine;
            rv += "Date:        " + TimeCard_Date.ToShortDateString() + Environment.NewLine;
            rv += "StartTime:  [" + TimeCard_StartTime.ToShortTimeString() + "]\n";
            rv += "EndTime:    [" + TimeCard_EndTime.ToShortTimeString() + "]\n";
            rv += "WorkHours:   " + TimeCard_WorkHours + Environment.NewLine;
            rv += "PayRate:     " + TimeCard_EmployeePayRate.ToString("C", System.Globalization.CultureInfo.CurrentCulture) + Environment.NewLine;
            rv += "ProjectId:   " + ProjectId + Environment.NewLine;
            rv += "ProjectName: " + ProjectName + Environment.NewLine;
            rv += "PhaseId:     " + PhaseId + Environment.NewLine;
            rv += "PhaseTitle:  " + PhaseTitle + Environment.NewLine;
            rv += "bReadOnly:  [" + TimeCard_bReadOnly.ToString() + "]\n";
            rv += "------------------------------------------------------\n";
            return rv;
        }
#endif
    }

    public sealed class TimeCardMap : ClassMap<TimeCard>
    {
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
