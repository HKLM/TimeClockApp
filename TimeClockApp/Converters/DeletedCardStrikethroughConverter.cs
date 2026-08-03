namespace TimeClockApp.Converters
{
    /// <summary>Converts shift statuses to strikethrough text decorations for deleted time cards.</summary>
    public class DeletedCardStrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ShiftStatus status && status == ShiftStatus.Deleted)
            {
                return TextDecorations.Strikethrough;
            }
            return TextDecorations.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is TextDecorations decorations && decorations.HasFlag(TextDecorations.None);
        }
    }
}
