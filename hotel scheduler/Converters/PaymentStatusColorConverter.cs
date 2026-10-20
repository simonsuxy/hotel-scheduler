using System.Globalization;
using DAL.Entities;

namespace hotel_scheduler.Converters
{
    public class PaymentStatusToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PaymentStatus status)
            {
                return status switch
                {
                    PaymentStatus.Paid => Color.FromArgb("#43A047"),     // Green
                    PaymentStatus.Unpaid => Color.FromArgb("#E53935"),   // Red
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