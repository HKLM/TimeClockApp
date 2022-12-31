using System.Globalization;

namespace TimeClockApp.Converters
{
    public class DateOnlyToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly d)
                return new DateTime(d.Year, d.Month, d.Day);
            else if (value is DateTime dt)
                return DateOnly.FromDateTime(dt);
            else if (value is string str)
            {
                if (DateOnly.TryParse(str, out DateOnly strTime))
                    return new DateTime(strTime.Year, strTime.Month, strTime.Day);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return DateOnly.FromDateTime(dt);
            else if (value is DateOnly d)
                return new DateTime(d.Year, d.Month, d.Day);
            if (value is string str)
            {
                if (DateOnly.TryParse(str, out DateOnly strTime))
                    return strTime;
            }
            return null;
        }
    }
}
