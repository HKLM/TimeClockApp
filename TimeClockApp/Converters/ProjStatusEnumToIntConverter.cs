using System.Globalization;

using TimeClockApp.Models;

namespace TimeClockApp.Converters
{
    public class ProjStatusEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and ProjectStatus)
            {
                return System.Convert.ToInt32(value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int)
                return (ProjectStatus)Enum.ToObject(typeof(ProjectStatus), @int);
            else if (value is string @str)
                return (ProjectStatus)Enum.ToObject(typeof(ProjectStatus), @str);
            else
                return value;
        }
    }
}
