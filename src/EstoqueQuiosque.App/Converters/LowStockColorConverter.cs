using System.Globalization;

namespace EstoqueQuiosque.App.Converters;

public class LowStockColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var abaixoDoMinimo = value is bool status && status;
        return abaixoDoMinimo ? Color.FromArgb("#EA580C") : Color.FromArgb("#16A34A");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
