namespace TimeClockApp.Converters
{
    public class EmploymentEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and EmploymentStatus)
            {
                return System.Convert.ToInt32(value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int)
                return (EmploymentStatus)Enum.ToObject(typeof(EmploymentStatus), @int);
            else if (value is string @str)
                return (EmploymentStatus)Enum.ToObject(typeof(EmploymentStatus), @str);
            else
                return value;
        }
    }
}
