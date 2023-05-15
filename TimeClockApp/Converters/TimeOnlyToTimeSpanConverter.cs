namespace TimeClockApp.Converters
{
    public class TimeOnlyToTimeSpanConverter : IValueConverter
    {
        /// <summary>
        /// TimeOnly -> TimeSpan
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and string str)
            {
                if (TimeSpan.TryParse(str, out TimeSpan strTime))
                    return strTime;
            }
            else if (value is not null and TimeOnly time)
            {
                return time.ToTimeSpan();
            }
            return null;
        }

        /// <summary>
        /// TimeSpan -> TimeOnly
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and TimeSpan time)
                return TimeOnly.FromTimeSpan(time);
            else if (value is not null and string str)
            {
                if (TimeOnly.TryParse(str, out var strTime))
                    return strTime;
            }
            return null;
        }
    }
}
