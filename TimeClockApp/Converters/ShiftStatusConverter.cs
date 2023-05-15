namespace TimeClockApp.Converters
{
    public class ShiftStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and ShiftStatus)
            {
                return Enum.GetName(typeof(ShiftStatus), (ShiftStatus)value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                try
                {
                    return (ShiftStatus)Enum.Parse(typeof(ShiftStatus), (string)value);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return value;
        }
    }
}
