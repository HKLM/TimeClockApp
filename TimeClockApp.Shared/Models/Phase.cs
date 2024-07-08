using System.ComponentModel.DataAnnotations;
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

#if DEBUG
        public override string ToString()
        {
            string rv = "\n--------------[  PhaseId: " + PhaseId + "  ]---------------------\n";
            rv += "PhaseTitle:  " + PhaseTitle + Environment.NewLine;
            rv += "------------------------------------------------------\n";
            return rv;
        }
#endif
    }

    public sealed class PhaseMap : ClassMap<Phase>
    {
        public PhaseMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.PhaseId);
            Map(m => m.PhaseTitle);
        }
    }
}
