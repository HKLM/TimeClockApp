using System.Globalization;

namespace TimeClockApp.Converters
{
    public class WorkHoursToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;
            if (value is decimal)
                return ((decimal)value).ToString("F1", CultureInfo.InvariantCulture);
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && (value == null || string.IsNullOrEmpty((string)value))) return value;
            string v = (string)value;
            if (decimal.TryParse(v, out decimal x))
                return x;

            return v;
        }
    }
}
