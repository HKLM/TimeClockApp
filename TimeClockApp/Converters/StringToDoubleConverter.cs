#nullable enable

namespace TimeClockApp.Converters
{
    public class StringToDoubleConverter : IValueConverter
    {
        private static readonly char[] TrimChars = { ' ', '$', ',' };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double @double)
                return @double.ToString("C");

            var stringValue = value?.ToString();
            if (string.IsNullOrEmpty(stringValue))
                return 0;

            return $"${stringValue}";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            if (string.IsNullOrEmpty(stringValue))
                return 0;

            var trimmed = stringValue.Trim(TrimChars);
            if (double.TryParse(trimmed, out var result))
                return result;

            return trimmed;
        }
    }
}
