using CsvHelper;

namespace TimeClockApp.Shared.Models
{
	[CsvRecord]
	public partial class Expense
    {
        public Expense() { }
        public Expense(int ProjectId, int PhaseId, double Amount, DateOnly ExpenseDate, string ExpenseProject = "", string ExpensePhase = "", string Memo = "", int ExpenseTypeId = 2, string ExpenseTypeCategoryName = "")
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
            this.ExpenseTypeCategoryName = string.IsNullOrEmpty(ExpenseTypeCategoryName) ? string.Empty : ExpenseTypeCategoryName;
        }

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
        /// Saved ExpenseTypeCategoryName at the time this expense was made.
        /// </summary>
        public string ExpenseTypeCategoryName { get; set; }

        /// <summary>
        /// Date of expense
        /// </summary>
        public DateOnly ExpenseDate { get; set; }

        /// <summary>
        /// Optional note about this expense.
        /// </summary>
        /// <remarks>Currently the only way to see this, is by editing the expense. This is not displayed
        /// any where else. TODO</remarks>
        [CsvColumn(Optional = true)]
        public string Memo { get; set; } = string.Empty;

        //[Required]
        public double Amount { get; set; }

        /// <summary>
        /// When false, this record is considered archived. No longer actively displayed.
        /// </summary>
        public bool IsRecent { get; set; }

		/// <summary>
		/// Saved Project Name at the time this expense was made.
		/// </summary>
		[CsvColumn(Optional = true)]
		public string ExpenseProject { get; set; } = string.Empty;

        /// <summary>
        /// Saved Project Title at the time this expense was made.
        /// </summary>
        [CsvColumn(Optional = true)]    
        public string ExpensePhase { get; set; } = string.Empty;

        [CsvIgnore]
        public virtual Project Project { get; set; }
        [CsvIgnore]
        public virtual Phase Phase { get; set; }
        [CsvIgnore]
        public virtual ExpenseType ExpenseType { get; set; }
    }
}