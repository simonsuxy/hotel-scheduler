using System.Globalization;

namespace hotel_scheduler.Converters
{
    public class ActivePageColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string activePage && parameter is string targetPage)
            {
                if (activePage.Equals(targetPage, StringComparison.OrdinalIgnoreCase))
                {
                    return Color.FromArgb("#F0F1FE"); // Active background highlight
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}