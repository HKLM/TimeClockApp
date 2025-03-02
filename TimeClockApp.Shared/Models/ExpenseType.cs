using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

#nullable enable

namespace TimeClockApp.Shared.Models
{
    public class ExpenseType
    {
        [Key]
        public int ExpenseTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

    public sealed class ExpenseTypeMap : ClassMap<ExpenseType>
    {
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        public ExpenseTypeMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.ExpenseTypeId);
            Map(m => m.CategoryName);
        }
    }
}
