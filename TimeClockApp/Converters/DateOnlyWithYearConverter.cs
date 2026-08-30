#nullable enable

namespace TimeClockApp.Converters
{
	public class DateOnlyWithYearConverter : IValueConverter
	{
		private static readonly CultureInfo UsEnglishCulture = CultureInfo.CreateSpecificCulture("en-US");

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return value is DateOnly d ? d.ToString(GetParameter(parameter), UsEnglishCulture) : null;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			return value switch
			{
				DateOnly d => d.ToShortDateString(),
				string str when DateOnly.TryParse(str, out DateOnly strTime) => strTime,
				_ => null
			};
		}

		string GetParameter(object? parameter) => parameter is not null and string str ? str : "MMM d ddd yyyy";
	}
}
