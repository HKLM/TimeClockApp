using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

namespace TimeClockApp.Shared.Models
{
    public class Expense : BaseEntity
    {
        public Expense() { }
        public Expense(int ProjectId, int PhaseId, double Amount, DateOnly ExpenseDate, string ExpenseProject = "", string ExpensePhase = "", string Memo = "", int ExpenseTypeId = 2, string ExpenseType_CategoryName = "")
        {
            this.ProjectId = ProjectId;
            this.PhaseId = PhaseId;
            this.ExpenseTypeId = ExpenseTypeId;
            this.ExpenseDate = ExpenseDate;
            this.Amount = Amount;
            this.IsRecent = true;
            this.Memo = Memo;
            this.ExpenseProject = string.IsNullOrEmpty(ExpenseProject) ? string.Empty : ExpenseProject;
            this.ExpensePhase = string.IsNullOrEmpty(ExpensePhase) ? string.Empty : ExpensePhase;
            this.ExpenseType_CategoryName = string.IsNullOrEmpty(ExpenseType_CategoryName) ? string.Empty : ExpenseType_CategoryName;
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
        /// A way to categorize expenses
        /// </summary>
        public int ExpenseTypeId { get; set; }

        /// <summary>
        /// Saved ExpenseType_CategoryName at the time this expense was made.
        /// </summary>
        public string ExpenseType_CategoryName { get; set; }

        /// <summary>
        /// Date of expense
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "DateOnly")]
        public DateOnly ExpenseDate { get; set; }

        /// <summary>
        /// Optional note about this expense.
        /// </summary>
        /// <remarks>Currently the only way to see this, is by editing the expense. This is not displayed
        /// any where else. TODO</remarks>
        public string Memo { get; set; } = string.Empty;

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double Amount { get; set; }

        /// <summary>
        /// When false, this record is considered archived. No longer actively displayed.
        /// </summary>
        public bool IsRecent { get; set; }

        /// <summary>
        /// Saved Project Name at the time this expense was made.
        /// </summary>
        public string ExpenseProject { get; set; } = string.Empty;

        /// <summary>
        /// Saved Project Title at the time this expense was made.
        /// </summary>
        public string ExpensePhase { get; set; } = string.Empty;

        public virtual Project Project { get; set; } 
        public virtual Phase Phase { get; set; }
        public virtual ExpenseType ExpenseType { get; set; }

#if DEBUG
        public override string ToString()
        {
            string rv = "\n--------------[  ExpenseId: " + ExpenseId + "  ]---------------------\n";
            rv += "ProjectId:       " + ProjectId + Environment.NewLine;
            rv += "PhaseId:         " + PhaseId + Environment.NewLine;
            rv += "ExpenseTypeId:   " + ExpenseTypeId + Environment.NewLine;
            rv += "ExpenseType_CategoryName:   " + ExpenseType_CategoryName + Environment.NewLine;
            rv += "ExpenseDate:     " + ExpenseDate.ToString() + Environment.NewLine;
            rv += "Memo:            " + Memo + Environment.NewLine;
            rv += "Amount:          " + Amount.ToString() + Environment.NewLine;
            rv += "IsRecent:        " + IsRecent.ToString() + Environment.NewLine;
            rv += "ExpenseProject:  " + ExpenseProject + Environment.NewLine;
            rv += "ExpensePhase:    " + ExpensePhase + Environment.NewLine;
            rv += "------------------------------------------------------\n";
            return rv;
        }
#endif
    }

    public sealed class ExpenseMap : ClassMap<Expense>
    {
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        public ExpenseMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.ExpenseId);
            Map(m => m.ProjectId);
            Map(m => m.PhaseId);
            Map(m => m.ExpenseTypeId);
            Map(m => m.ExpenseType_CategoryName);
            Map(m => m.ExpenseDate);
            Map(m => m.Memo).Optional();
            Map(m => m.Amount);
            Map(m => m.IsRecent);
            Map(m => m.ExpenseProject).Optional();
            Map(m => m.ExpensePhase).Optional();
        }
    }
}