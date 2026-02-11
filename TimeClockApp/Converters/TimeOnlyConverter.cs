#nullable enable

namespace TimeClockApp.Converters
{
    public class TimeOnlyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                string str => TimeOnly.TryParse(str, out var time) ? time : null,
                TimeOnly time => time.ToShortTimeString(),
                _ => null
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                TimeOnly time => time.ToShortTimeString(),
                string str => TimeOnly.TryParse(str, out var time) ? time : null,
                _ => null
            };
        }
    }
}
