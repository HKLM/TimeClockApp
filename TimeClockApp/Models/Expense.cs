using System.ComponentModel.DataAnnotations;

using CsvHelper.Configuration;

namespace TimeClockApp.Models
{
    //TODO move to database table so that user can modify the entries
    public enum ExpenseType
    {
        [Description("Income")]
        Income = 0,
        [Description("Materials")]
        Materials = 1,
        [Description("Payroll")]
        Payroll = 2,
        [Description("Dumps")]
        Dumps = 3,
        [Description("Toll-Gas")]
        Toll = 4,
        [Description("Overhead")]
        Overhead = 5,
        [Description("Service")]
        Service = 6,
        [Description("Permit")]
        Permit = 7,
        [Description("Misc")]
        Misc = 8,
        [Description("Refund")]
        Refund = 9,
        [Description("Deleted")]
        Deleted = 10
    }

    public class Expense : BaseEntity
    {
        public Expense() { }
        public Expense(int projectId, int phaseId, double amount, DateOnly expenseDate, string projectName = "", string phaseTitle = "", string memo = "", ExpenseType category = ExpenseType.Materials)
        {
            ProjectId = projectId;
            PhaseId = phaseId;
            Memo = memo;
            Amount = amount;
            Category = category;
            ExpenseDate = expenseDate;
            IsRecent = true;
            ExpenseProject = projectName == "" ? null : projectName;
            ExpensePhase = phaseTitle == "" ? null : phaseTitle;
        }

        [Key]
        public int ExpenseId { get; set; }
        /// <summary>
        /// The Project this Expense is associated with.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The phase of the Project this Expense is associated with.
        /// </summary>
        public int PhaseId { get; set; }

        /// <summary>
        /// Date of expense
        /// </summary>
        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "DateOnly")]
        public DateOnly ExpenseDate { get; set; }

        /// <summary>
        /// Optional note about this expense.
        /// </summary>
        /// <remarks>Currently the only way to see this, is by editing the expense. This is not displayed
        /// any where else. TODO</remarks>
        public string Memo { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double Amount { get; set; }

        /// <summary>
        /// A way to categorize expenses
        /// </summary>
        [Required]
        public ExpenseType Category { get; set; }

        /// <summary>
        /// When false, this record is considered archived. No longer actively displayed.
        /// </summary>
        public bool IsRecent { get; set; }

        /// <summary>
        /// Saved Project Name at the time this expense was made.
        /// </summary>
        public string ExpenseProject { get; set; } = null;

        /// <summary>
        /// Saved Project Title at the time this expense was made.
        /// </summary>
        public string ExpensePhase { get; set; } = null;

        public virtual Project Project { get; set; }
        public virtual Phase Phase { get; set; }

        public override string ToString()
        {
            string rv = "\n--------------[  ExpenseId: " + ExpenseId + "  ]---------------------\n";
            rv += "ProjectId:       " + ProjectId + Environment.NewLine;
            rv += "PhaseId:         " + PhaseId + Environment.NewLine;
            rv += "ExpenseDate:     " + ExpenseDate.ToString() + Environment.NewLine;
            rv += "Memo:            " + Memo + Environment.NewLine;
            rv += "Amount:          " + Amount.ToString() + Environment.NewLine;
            rv += "Category:        " + Category.ToString() + Environment.NewLine;
            rv += "IsRecent:        " + IsRecent.ToString() + Environment.NewLine;
            rv += "ExpenseProject:  " + ExpenseProject + Environment.NewLine;
            rv += "ExpensePhase:    " + ExpensePhase + Environment.NewLine;
            rv += "------------------------------------------------------\n";
            return rv;
        }
    }

    public sealed class ExpenseMap : ClassMap<Expense>
    {
        public ExpenseMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.ExpenseId);
            Map(m => m.ProjectId);
            Map(m => m.PhaseId);
            Map(m => m.ExpenseDate);
            Map(m => m.Memo).Optional();
            Map(m => m.Amount);
            Map(m => m.Category).Name("Category", "Catagory").Default(ExpenseType.Materials);
            Map(m => m.IsRecent);
            Map(m => m.ExpenseProject).Optional();
            Map(m => m.ExpensePhase).Optional();
        }
    }
}