namespace TimeClockApp.Converters
{
    public class DateOnlyToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                DateTime dt => DateOnly.FromDateTime(dt),
                string str when DateOnly.TryParse(str, out var strTime) => strTime.ToDateTime(TimeOnly.MinValue),
                _ => null
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DateTime dt => DateOnly.FromDateTime(dt),
                DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                string str when DateOnly.TryParse(str, out var strTime) => strTime,
                _ => null
            };
        }
    }
}
