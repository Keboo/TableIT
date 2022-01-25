using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace TableIT.Remote.Converters
{
    public class NotNegativeValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is double dbl && dbl >= 0.0;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
