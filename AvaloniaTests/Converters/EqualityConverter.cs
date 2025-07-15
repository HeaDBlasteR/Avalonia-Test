using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaTests.Converters
{
    public class EqualityConverter : IValueConverter
    {
        public static EqualityConverter Instance { get; } = new EqualityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
