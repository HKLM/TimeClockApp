using System.ComponentModel.DataAnnotations;

using CsvHelper.Configuration;

using Microsoft.EntityFrameworkCore;

#nullable enable

namespace TimeClockApp.Shared.Models
{
    /// <summary>
    /// Phases are individual tasks, that as a whole, make up the project.
    /// </summary>
    [Index(nameof(PhaseTitle), IsUnique = true)]
    public class Phase
    {
        public Phase() { }
        public Phase(string phaseTitle)
        {
            PhaseTitle = phaseTitle;
        }

        public Phase(int phaseId, string phaseTitle)
        {
            PhaseId = phaseId;
            PhaseTitle = phaseTitle;
        }

        [Key]
        public int PhaseId { get; set; }

        [Required]
        public string PhaseTitle { get; set; } = string.Empty;

        public virtual ICollection<TimeCard> TimeCards { get; set; } = new HashSet<TimeCard>();

        public override string ToString()
        {
            string rv = Environment.NewLine + "--------------[  PhaseId: " + PhaseId + "  ]---------------------" + Environment.NewLine;
            rv += "PhaseTitle:  " + PhaseTitle + Environment.NewLine;
            rv += "------------------------------------------------------" + Environment.NewLine;
            return rv;
        }
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
