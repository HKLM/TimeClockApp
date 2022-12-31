using System.ComponentModel.DataAnnotations;

using CsvHelper.Configuration;

using Microsoft.EntityFrameworkCore;

namespace TimeClockApp.Models
{
    /// <summary>
    /// Record of time worked, overtime if any, and wages earned
    /// </summary>
    [Index(nameof(TimeCardId), IsUnique = true)]
    public partial class Wages
    {
        public Wages() { }

        public Wages(int timeCardId)
        {
            TimeCardId = timeCardId;
            TotalHours = 0;
            RegularHours = 0;
            OTHours = 0;
            OT2Hours = 0;
            RegPay = 0;
            OT_Pay = 0;
            OT2_Pay = 0;
            TotalWages = 0;
        }

        [Key]
        public int WagesId { get; set; }

        [Required]
        public int TimeCardId { get; set; }

        /// <summary>
        /// The overall Total Hours of the associated TimeCard
        /// </summary>
        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double TotalHours { get; set; }

        /// <summary>
        /// Of the TotalHours, this would be the first 8 hours.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double RegularHours { get; set; }

        /// <summary>
        /// Overtime. After 8 hours of work, the next 2 hours in a single day, are consited overtime.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double OTHours { get; set; }

        /// <summary>
        /// Double Overtime. Any work done after 10 hours in a single day, is consited double overtime.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double OT2Hours { get; set; }

        /// <summary>
        /// This is the amount of wages for the first 8 hours [Employee_PayRate * RegularHours]
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double RegPay { get; set; }

        /// <summary>
        /// Amount of wages earned during Overtime [(Employee_PayRate * 1.5) * OTHours]
        /// </summary>
        [Comment("Overtime Time and a Half Rate 1.5x")]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double OT_Pay { get; set; }

        /// <summary>
        /// Amount of wages earned during Double Overtime [(Employee_PayRate * 2) * OTHours]
        /// </summary>
        [Comment("Double Overtime Rate 2x")]
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double OT2_Pay { get; set; }

        /// <summary>
        /// TotalWages = RegPay + OT_Pay + OT2_Pay
        /// </summary>
        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "double")]
        public double TotalWages { get; set; }

        public virtual TimeCard TimeCard { get; set; }

        /// <summary>
        /// To quickly reset this entity back to zero
        /// </summary>
        public void Reset()
        {
            this.TotalHours = 0;
            this.RegularHours = 0;
            this.OTHours = 0;
            this.OT2Hours = 0;
            this.RegPay = 0;
            this.OT_Pay = 0;
            this.OT2_Pay = 0;
            this.TotalWages = 0;
        }

        public override string ToString()
        {
            string rv = Environment.NewLine + "--------------[  WagesId: " + WagesId + "  ]---------------------" + Environment.NewLine;
            rv += "TimeCardId:   " + TimeCardId + Environment.NewLine;
            rv += "TotalHours:   " + TotalHours + Environment.NewLine;
            rv += "RegularHours: " + RegularHours + Environment.NewLine;
            rv += "OTHours:      " + OTHours + Environment.NewLine;
            rv += "OT2Hours:     " + OT2Hours + Environment.NewLine;
            rv += "RegPay:       " + RegPay + Environment.NewLine;
            rv += "OT_Pay:       " + OT_Pay + Environment.NewLine;
            rv += "OT2_Pay:      " + OT2_Pay + Environment.NewLine;
            rv += "TotalWages:   " + TotalWages + Environment.NewLine;
            rv += "------------------------------------------------------" + Environment.NewLine;
            return rv;
        }
    }

    public sealed class WagesMap : ClassMap<Wages>
    {
        public WagesMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.WagesId);
            Map(m => m.TimeCardId);
            Map(m => m.TotalHours);
            Map(m => m.RegularHours);
            Map(m => m.OTHours);
            Map(m => m.OT2Hours);
            Map(m => m.RegPay);
            Map(m => m.OT_Pay);
            Map(m => m.OT2_Pay);
            Map(m => m.TotalWages);
        }
    }
}
