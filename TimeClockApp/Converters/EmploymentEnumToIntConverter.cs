namespace TimeClockApp.Converters
{
    public class EmploymentEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EmploymentStatus status)
            {
                return (int)(object)status;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                int @int => (EmploymentStatus)@int,
                string @str => Enum.Parse<EmploymentStatus>(@str),
                _ => value
            };
        }
    }
}
