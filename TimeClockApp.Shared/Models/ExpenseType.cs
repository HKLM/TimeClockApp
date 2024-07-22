using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Shared.Models
{
    [Index(nameof(CategoryName), IsUnique = true)]
    public class ExpenseType
    {
        [Key]
        public int ExpenseTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        public virtual ICollection<Expense> Expenses { get; set; }
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
