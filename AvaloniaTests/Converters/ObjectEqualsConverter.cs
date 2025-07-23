using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaTests.Converters
{
    public class ObjectEqualsConverter : IValueConverter
    {
        public static ObjectEqualsConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Equals(value, parameter);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return parameter;
            
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}