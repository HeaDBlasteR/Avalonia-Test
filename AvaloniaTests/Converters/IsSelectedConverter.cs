using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvaloniaTests.Converters
{
    public class IsSelectedConverter : IMultiValueConverter
    {
        public static IsSelectedConverter Instance { get; } = new();

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 && values[0] is Guid answerId && values[1] is Guid selectedAnswerId)
            {
                return answerId == selectedAnswerId;
            }
            
            if (values.Count == 2 && values[0] is Guid id && values[1] == null)
            {
                return false;
            }
            
            return false;
        }
    }
}