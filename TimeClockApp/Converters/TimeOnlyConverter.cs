#nullable enable

namespace TimeClockApp.Converters
{
    public class TimeOnlyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not null and string str)
            {
                if (TimeOnly.TryParse(str, out var strTime))
                    return strTime;
            }
            else if (value is not null and TimeOnly time)
            {
                return time.ToShortTimeString();
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not null and TimeOnly time)
            {
                return time.ToShortTimeString();
            }
            else if (value is not null and string str)
            {
                if (TimeOnly.TryParse(str, out var strTime))
                    return strTime;
            }
            return null;
        }
    }
}
