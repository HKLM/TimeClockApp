namespace TimeClockApp.Converters
{
    public class DateOnlyWithYearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not null and DateOnly d ? d.ToString("MMM d ddd yyyy", CultureInfo.CreateSpecificCulture("en-US")) : (object)null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null and DateOnly d)
                return d.ToShortDateString();
            else if (value is not null and string str)
            {
                if (DateOnly.TryParse(str, out var strTime))
                    return strTime;
            }
            return null;
        }
    }
}
