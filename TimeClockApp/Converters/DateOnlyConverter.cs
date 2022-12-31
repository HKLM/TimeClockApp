using System.Globalization;

namespace TimeClockApp.Converters
{
    public class DateOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and string str)
            {
                if (DateOnly.TryParse(str, out var strTime))
                    return strTime;
            }
            else if (value is not null and DateOnly d)
                return d.ToString("MMM d ddd", CultureInfo.CreateSpecificCulture("en-US"));

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and DateOnly d)
                return d.ToShortDateString();
            else if (value is not null and string str)
            {
                if (DateOnly.TryParse(str, out var strTime))
                    return strTime;
            }
            return null;
        }
    }
}
