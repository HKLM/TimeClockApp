#nullable enable

namespace TimeClockApp.Converters
{
    public class DateOnlyConverter : IValueConverter
    {
        private static readonly CultureInfo EnUsCulture = CultureInfo.CreateSpecificCulture("en-US");

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                string str => DateOnly.TryParse(str, out var date) ? date : null,
                DateOnly d => d.ToString("MMM d ddd", EnUsCulture),
                _ => null
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                DateOnly d => d.ToShortDateString(),
                string str => DateOnly.TryParse(str, out var date) ? date : null,
                _ => null
            };
        }
    }
}
