using System.Globalization;
using DAL.Entities;

namespace hotel_scheduler.Converters
{
    public class RoomStatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is RoomStatus status)
            {
                return status switch
                {
                    RoomStatus.Active => Color.FromArgb("#43A047"),      // Green
                    RoomStatus.Maintenance => Color.FromArgb("#FB8C00"), // Orange
                    RoomStatus.Inactive => Color.FromArgb("#E53935"),    // Red
                    _ => Color.FromArgb("#9E9E9E")
                };
            }
            return Color.FromArgb("#9E9E9E");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}