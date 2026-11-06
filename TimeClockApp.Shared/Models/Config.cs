using CsvHelper;
#nullable enable

namespace TimeClockApp.Shared.Models
{
	/// <summary>
	/// Settings data storage. Each Setting has a Id and Name to Identify each setting.
	/// Values are either the StringValue or IntValue.
	/// </summary>
	[CsvRecord]
	public partial class Config
	{
		public int ConfigId { get; set; }

        /// <summary>
        /// String name to help identify what this row is for
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Optional-This settings value as a string.
        /// </summary>
        /// <remarks>Default is null</remarks>
        [CsvColumn(Optional = true)]
        public string? StringValue { get; set; }

		/// <summary>
		/// Optional-This settings value as a integer
		/// </summary>
		/// <remarks>Default is null</remarks>
		[CsvColumn(Optional = true)]
		public int? IntValue { get; set; }

        /// <summary>
        /// Optional brief description of this config item.
        /// </summary>
        [CsvColumn(Optional = true)]
        public string? Hint { get; set; }
    }
}
