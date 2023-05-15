namespace TimeClockApp.Converters
{
    public class ReverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return !(bool)value;
            else if ((bool?)value != null)
                return !(bool?)value;
            else if (value is int @int)
                return !(@int == 1);
            else if (value is int?)
                return !((int?)value == 1);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return !(bool)value;
            else if ((bool?)value != null)
                return !(bool?)value;
            else if (value is int @int)
                return !(@int == 1);
            else if (value is int?)
                return !((int?)value == 1);

            return value;
        }

    }
}
