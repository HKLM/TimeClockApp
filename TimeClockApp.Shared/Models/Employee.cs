using System.ComponentModel;
using CsvHelper;
#nullable enable

namespace TimeClockApp.Shared.Models
{
    /// <summary>
    /// Employment Status
    /// </summary>
    public enum EmploymentStatus
    {
        /// <summary>
        /// Active Employee
        /// </summary>
        [Description("Employed")]
        Employed = 0,
        /// <summary>
        /// Still on payroll, but not actively working
        /// </summary>
        [Description("Inactive")]
        Inactive = 1,
        /// <summary>
        /// Time off, vacation, or other scheduled time off
        /// </summary>
        [Description("Leave")]
        Leave = 2,
        /// <summary>
        /// No longer Employed (Fired, quit, etc)
        /// </summary>
        [Description("NotEmployed")]
        NotEmployed = 3,
        /// <summary>
        /// Delete this Employee. Employee will no longer be displayed on any screen.
        /// </summary>
        [Description("Deleted")]
        Deleted = 4
    }

	[CsvRecord]
	public partial class Employee
    {
        public Employee()
        {
            TimeCards = new HashSet<TimeCard>();
        }

        public Employee(string employee_Name, double employee_PayRate, EmploymentStatus employee_Employed, string jobTitle)
        {
            Employee_Name = employee_Name;
            Employee_PayRate = employee_PayRate;
            Employee_Employed = employee_Employed;
            JobTitle = jobTitle ?? string.Empty;
            TimeCards = [];
        }

        public int EmployeeId { get; set; }

        public string Employee_Name { get; set; } = string.Empty;

        /// <summary>
        /// Employees Rate of Pay.
        /// </summary>
        public double Employee_PayRate { get; set; }

        /// <summary>
        /// Valve determines if this Employee is displayed on the Home TimeCards page.
        /// </summary>
        public EmploymentStatus Employee_Employed { get; set; }

        public string JobTitle { get; set; } = string.Empty;

		[CsvIgnore]
		public virtual ICollection<TimeCard> TimeCards { get; set; }
    }

}
