using System.Globalization;

using TimeClockApp.Models;

namespace TimeClockApp.Converters
{
    public class ExpenseEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and ExpenseType)
            {
                return System.Convert.ToInt32(value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int)
                return (ExpenseType)Enum.ToObject(typeof(ExpenseType), @int);
            else if (value is string @str)
                return (ExpenseType)Enum.ToObject(typeof(ExpenseType), @str);
            else
                return value;
        }
    }
}
