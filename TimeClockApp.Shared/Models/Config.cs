using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;
#nullable enable 

namespace TimeClockApp.Shared.Models
{
    /// <summary>
    /// Settings data storage. Each Setting has a Id and Name to Identify each setting. 
    /// Values are either the StringValue or IntValue.
    /// </summary>
    public class Config
    {
        [Key]
        public int ConfigId { get; set; }

        /// <summary>
        /// String name to help identify what this row is for
        /// </summary>
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// Optional-This settings value as a string.
        /// </summary>
        /// <remarks>Default is null</remarks>
        public string? StringValue { get; set; }

        /// <summary>
        /// Optional-This settings value as a integer
        /// </summary>
        /// <remarks>Default is null</remarks>
        public int? IntValue { get; set; }

        /// <summary>
        /// Optional brief description of this config item.
        /// </summary>
        public string? Hint { get; set; }

#if DEBUG
        public override string ToString()
        {
            string rv = "\n--------------[  ConfigId: " + ConfigId + "  ]---------------------\n";
            rv += "Name:        " + Name + Environment.NewLine;
            rv += "StringValue: " + StringValue + Environment.NewLine;
            rv += "IntValue:    " + IntValue?.ToString() + Environment.NewLine;
            rv += "Hint:        " + Hint + Environment.NewLine;
            rv += "------------------------------------------------------\n";
            return rv;
        }
#endif
    }

    public sealed class ConfigMap : ClassMap<Config>
    {
        public ConfigMap()
        {
            //AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.ConfigId);
            Map(m => m.Name);
            Map(m => m.StringValue).Optional();
            Map(m => m.IntValue).Optional();
            Map(m => m.Hint).Optional();
        }
    }
}
