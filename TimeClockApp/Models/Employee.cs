using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using CsvHelper.Configuration;

using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Models
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
        [Description("NotEmployeed")]
        NotEmployeed = 3,
        /// <summary>
        /// Delete this Employee. Employee will no longer be displayed on any screen.
        /// </summary>
        [Description("Deleted")]
        Deleted = 4
    }

    [Index(nameof(Employee_Name), IsUnique = true)]
    public class Employee
    {

        public Employee()
        {
            TimeCards = new HashSet<TimeCard>();
        }

        public Employee(string employee_Name, double employee_PayRate, string jobTitle)
        {
            Employee_Name = employee_Name;
            Employee_PayRate = employee_PayRate;
            JobTitle = jobTitle;
            Employee_Employed = EmploymentStatus.Employed;
        }

        public Employee(int employeeId, string employee_Name, double employee_PayRate, EmploymentStatus employee_Employed, string jobTitle)
        {
            EmployeeId = employeeId;
            Employee_Name = employee_Name;
            Employee_PayRate = employee_PayRate;
            Employee_Employed = employee_Employed;
            JobTitle = jobTitle;
        }

        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Employee_Name { get; set; }

        /// <summary>
        /// Employees Rate of Pay.
        /// </summary>
        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double Employee_PayRate { get; set; }

        /// <summary>
        /// Valve determins if this Employee is displayed on the Home TimeCards page.
        /// </summary>
        [Required]
        public EmploymentStatus Employee_Employed { get; set; }

        [StringLength(50)]
        public string JobTitle { get; set; }

        public virtual ICollection<TimeCard> TimeCards { get; set; }

        public override string ToString()
        {
            string rv = Environment.NewLine + "--------------[  EmployeeId: " + EmployeeId + "  ]---------------------" + Environment.NewLine;
            rv += "Employee_Name:     " + Employee_Name + Environment.NewLine;
            rv += "Employee_PayRate:  " + Employee_PayRate.ToString("C", System.Globalization.CultureInfo.CurrentCulture) + Environment.NewLine;
            rv += "Employee_Employed: " + Employee_Employed.ToString() + Environment.NewLine;
            rv += "JobTitle:          " + JobTitle + Environment.NewLine;
            rv += "------------------------------------------------------" + Environment.NewLine;
            return rv;
        }
    }

    public sealed class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.EmployeeId);
            Map(m => m.Employee_Name);
            Map(m => m.Employee_PayRate);
            Map(m => m.Employee_Employed);
            Map(m => m.JobTitle);
        }
    }
}
