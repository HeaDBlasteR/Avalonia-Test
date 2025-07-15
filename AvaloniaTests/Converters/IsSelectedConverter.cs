using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaloniaTests.Converters
{
    public class IsSelectedConverter : IValueConverter
    {
        public static IsSelectedConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Guid answerId && parameter is Guid selectedAnswerId)
            {
                return answerId == selectedAnswerId;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return parameter;
            }
            return AvaloniaProperty.UnsetValue;
        }
    }
}