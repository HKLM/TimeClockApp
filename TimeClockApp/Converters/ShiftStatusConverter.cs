namespace TimeClockApp.Converters
{
    using TimeClockApp.Shared.Models;

    public class ShiftStatusConverter : IValueConverter
    {
        private static readonly Dictionary<ShiftStatus, string> StatusToNameCache = 
            Enum.GetValues<ShiftStatus>()
                .Cast<ShiftStatus>()
                .ToDictionary(s => s, s => Enum.GetName(typeof(ShiftStatus), s) ?? string.Empty);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ShiftStatus status)
            {
                return StatusToNameCache.TryGetValue(status, out var name) ? name : value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string statusString && !string.IsNullOrWhiteSpace(statusString))
            {
                if (Enum.TryParse<ShiftStatus>(statusString, ignoreCase: true, out var result))
                {
                    return result;
                }
            }
            return value;
        }
    }
}
