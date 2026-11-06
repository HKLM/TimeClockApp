using CsvHelper;

#nullable enable

namespace TimeClockApp.Shared.Models
{
	[CsvRecord]
	public partial class ExpenseType
    {
        public int ExpenseTypeId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

		[CsvIgnore]
		public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
