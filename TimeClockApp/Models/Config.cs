using System.ComponentModel.DataAnnotations;

using CsvHelper.Configuration;

namespace TimeClockApp.Models
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
        public string Name { get; set; }

        /// <summary>
        /// Optional-This settings value as a string.
        /// </summary>
        /// <remarks>Default is null</remarks>
        public string StringValue { get; set; }

        /// <summary>
        /// Optional-This settings value as a integer
        /// </summary>
        /// <remarks>Default is null</remarks>
        public int? IntValue { get; set; }

        /// <summary>
        /// Optional brief description of this config item.
        /// </summary>
        public string Hint { get; set; }

        public override string ToString()
        {
            string rv = Environment.NewLine + "--------------[  ConfigId: " + ConfigId + "  ]---------------------" + Environment.NewLine;
            rv += "Name:        " + Name + Environment.NewLine;
            rv += "StringValue: " + StringValue + Environment.NewLine;
            rv += "IntValue:    " + IntValue.Value.ToString() + Environment.NewLine;
            rv += "Hint:        " + Hint + Environment.NewLine;
            rv += "------------------------------------------------------" + Environment.NewLine;
            return rv;
        }
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
