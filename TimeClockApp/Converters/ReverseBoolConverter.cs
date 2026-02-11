#nullable enable

namespace TimeClockApp.Converters
{
    public class ReverseBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => ReverseValue(value, true);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => ReverseValue(value, false);

        private static object? ReverseValue(object? value, bool nullDefault)
        {
            return value switch
            {
                null => nullDefault,
                bool b => !b,
                int i => !(i == 1),
                _ => value
            };
        }
    }
}
