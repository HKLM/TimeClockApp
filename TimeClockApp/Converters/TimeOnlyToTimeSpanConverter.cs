#nullable enable

namespace TimeClockApp.Converters
{
    public class TimeOnlyToTimeSpanConverter : IValueConverter
    {
        /// <summary>
        /// TimeOnly -> TimeSpan
        /// </summary>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value switch
            {
                TimeOnly time => time.ToTimeSpan(),
                string str when TimeSpan.TryParse(str, culture, out var parsed) => parsed,
                _ => null
            };

        /// <summary>
        /// TimeSpan -> TimeOnly
        /// </summary>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value switch
            {
                TimeSpan time => TimeOnly.FromTimeSpan(time),
                string str when TimeOnly.TryParse(str, culture, out var parsed) => parsed,
                _ => null
            };
    }
}
