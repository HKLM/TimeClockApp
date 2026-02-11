namespace TimeClockApp.Converters
{
    public class ProjStatusEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectStatus status)
            {
                return (int)status;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                int @int => (ProjectStatus)@int,
                string @str when int.TryParse(@str, out var parsedInt) => (ProjectStatus)parsedInt,
                _ => value
            };
        }
    }
}
