using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Shared.Models
{
    /// <summary>
    /// Phases are individual tasks, that as a whole, make up the project.
    /// </summary>
    [Index(nameof(PhaseTitle), IsUnique = true)]
    public class Phase
    {
        public Phase() { }
        public Phase(string PhaseTitle)
        {
            this.PhaseTitle = PhaseTitle;
        }

        public Phase(int PhaseId, string PhaseTitle)
        {
            this.PhaseId = PhaseId;
            this.PhaseTitle = PhaseTitle;
        }

        [Key]
        public int PhaseId { get; set; }

        [Required]
        public string PhaseTitle { get; set; } = string.Empty;

        public virtual ICollection<TimeCard> TimeCards { get; set; } = new HashSet<TimeCard>();
        public virtual ICollection<Expense> Expenses { get; set; } = new HashSet<Expense>();
    }

    public sealed class PhaseMap : ClassMap<Phase>
    {
        [RequiresUnreferencedCode("Calls DynamicBehavior for Import or Export to CSV.")]
        public PhaseMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.PhaseId);
            Map(m => m.PhaseTitle);
        }
    }
}
