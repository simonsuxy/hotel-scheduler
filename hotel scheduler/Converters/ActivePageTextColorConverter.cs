using System.Globalization;

namespace hotel_scheduler.Converters
{
    public class ActivePageTextColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string activePage && parameter is string targetPage)
            {
                if (activePage.Equals(targetPage, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.FromArgb("#4C4EE7"); // Active text accent
                }
            }
            return Color.FromArgb("#1A1A1A"); // Default text color
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}