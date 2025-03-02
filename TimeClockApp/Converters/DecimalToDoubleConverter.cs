#nullable enable
namespace TimeClockApp.Converters
{
    public class DecimalToDoubleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return null;
            else if (value is decimal)
                return decimal.ToDouble((decimal)value);
            else if (value is double x)
                return System.Convert.ToDouble((decimal)x);

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return null;
            else if (value is double)
                return System.Convert.ToDouble((decimal)value);
            else if (value is decimal)
                return decimal.ToDouble((decimal)value);

            return value;
        }
    }
}
