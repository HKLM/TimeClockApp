namespace TimeClockApp.Converters
{
    public class ShiftStatusEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not null and ShiftStatus ? System.Convert.ToInt32(value) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int @int ? (ShiftStatus)Enum.ToObject(typeof(ShiftStatus), @int) : value;
        }
    }
}
