using Avalonia.Data.Converters;
using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AvaloniaTests.Converters
{
    public class AnswerStatusConverter : IValueConverter
    {
        public static AnswerStatusConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Question question && parameter is TestResult result)
            {
                if (result.UserAnswers.TryGetValue(question.Id, out var userAnswer))
                {
                    return userAnswer == question.CorrectAnswerId ? "✅ Верно" : "❌ Неверно";
                }
                return "Не отвечено";
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }

    // Compares value with parameter (IValueConverter) or two bound values (IMultiValueConverter).
    // ConverterParameter can't be a binding, so compare two bound values through a MultiBinding.
    public class EqualityConverter : IValueConverter, IMultiValueConverter
    {
        public static EqualityConverter Instance { get; } = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ToResult(value != null && value.Equals(parameter), targetType);
        }

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var isEqual = values.Count == 2 && values[0] != null && values[0]!.Equals(values[1]);
            return ToResult(isEqual, targetType);
        }

        private static object ToResult(bool isEqual, Type targetType)
        {
            if (targetType == typeof(Avalonia.Media.Brush) || targetType == typeof(Avalonia.Media.IBrush))
            {
                return isEqual ?
                    new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#27AE60")) :
                    new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#BDC3C7"));
            }

            return isEqual;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}