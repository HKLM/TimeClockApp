namespace TimeClockApp.Converters
{
    /// <summary>Used by the TimeCardManagerPage to convert shift statuses to colors. Colors are defined in the Colors.xaml file.</summary>
    public class ShiftStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color displayColor = Color.FromRgb(110, 110, 110); // Default color
            if (value is ShiftStatus status)
            {
                bool foundColor;

                switch (status)
                {
                    case ShiftStatus.ClockedIn:
                        foundColor = App.Current.Resources.TryGetValue("TertiaryContainerDark", out object ClockedInColor);
                        if (foundColor)
                        {
                            displayColor = (Color)ClockedInColor;
                        }
                        break;
                    case ShiftStatus.ClockedOut:
                        foundColor = App.Current.Resources.TryGetValue("TertiaryContainer", out object ClockedOutColor);
                        if (foundColor)
                        {
                            displayColor = (Color)ClockedOutColor;
                        }
                        break;
                    case ShiftStatus.Paid:
                        foundColor = App.Current.Resources.TryGetValue("Primary", out object PaidColor);
                        if (foundColor)
                        {
                            displayColor = (Color)PaidColor;
                        }
                        break;
                    case ShiftStatus.Deleted:
                        foundColor = App.Current.Resources.TryGetValue("FireBrickAccent", out object DeletedColor);
                        if (foundColor)
                        {
                            displayColor = (Color)DeletedColor;
                        }
                        break;
                    default:
                        foundColor = App.Current.Resources.TryGetValue("SecondaryDarkText", out object DefaultColor);
                        if (foundColor)
                        {
                            displayColor = (Color)DefaultColor;
                        }
                        break;
                }
            }

            return displayColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
