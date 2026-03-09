using System.Globalization;

namespace EstoqueQuiosque.App.Converters;

public class LowStockColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var abaixoDoMinimo = value is bool status && status;
        return abaixoDoMinimo ? Color.FromArgb("#F87171") : Color.FromArgb("#4ADE80");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
