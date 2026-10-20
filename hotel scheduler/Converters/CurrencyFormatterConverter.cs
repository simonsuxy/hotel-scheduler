using System.Globalization;

namespace hotel_scheduler.Converters
{
    public class CurrencyFormatterConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                string symbol = parameter?.ToString() ?? "€";
                return $"{amount:N2} {symbol}";
            }
            return "0.00 €";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}