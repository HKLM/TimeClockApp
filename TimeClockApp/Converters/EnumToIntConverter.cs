namespace TimeClockApp.Converters
{
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and ShiftStatus)
            {
                return System.Convert.ToInt32(value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int @int)
                return (ShiftStatus)Enum.ToObject(typeof(ShiftStatus), @int);
            else if (value is string @str)
                return (ShiftStatus)Enum.ToObject(typeof(ShiftStatus), @str);
            else
                return value;
        }
    }
}
