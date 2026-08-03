using static TimeClockApp.Shared.Enums;

namespace TimeClockApp.Converters
{
    public class AppThemeEnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is not null and AppThemeType ? System.Convert.ToInt32(value) : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int @int ? (AppThemeType)Enum.ToObject(typeof(AppThemeType), @int) : value;
        }
    }
}
