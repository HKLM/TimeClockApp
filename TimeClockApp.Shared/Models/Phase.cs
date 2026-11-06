using CsvHelper;

namespace TimeClockApp.Shared.Models
{
	/// <summary>
	/// Phases are individual tasks, that as a whole, make up the project.
	/// </summary>
	[CsvRecord]
	public partial class Phase
    {
        public Phase() { }
        public Phase(string PhaseTitle)
        {
            this.PhaseTitle = PhaseTitle;
        }

        public int PhaseId { get; set; }

        public string PhaseTitle { get; set; } = string.Empty;

		[CsvIgnore]
		public virtual ICollection<TimeCard> TimeCards { get; set; } = new HashSet<TimeCard>();
		[CsvIgnore]
		public virtual ICollection<Expense> Expenses { get; set; } = new HashSet<Expense>();
    }
}
