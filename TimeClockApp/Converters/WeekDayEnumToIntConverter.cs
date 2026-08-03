namespace TimeClockApp.Converters
{
    public class WeekDayEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not null and DayOfWeek ? (int)value : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int ? (DayOfWeek)Enum.ToObject(typeof(DayOfWeek), (int)value) : value;

        }
    }
}
