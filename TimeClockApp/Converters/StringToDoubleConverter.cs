#nullable enable

namespace TimeClockApp.Converters
{
    public class StringToDoubleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()) || value.ToString() == "") return 0;
            if (value is double @double)
                return @double.ToString("C");

            return "$" + value.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return 0;

            string v = (string)value;
            string z = v.Trim(new Char[] { ' ', '$', ',' });
            if (double.TryParse(z, out double x))
                return x;

            return z;
        }
    }
}
