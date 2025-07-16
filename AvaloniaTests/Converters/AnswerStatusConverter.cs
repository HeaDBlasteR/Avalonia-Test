using Avalonia.Data.Converters;
using AvaloniaTests.Models;
using System;
using System.Globalization;

namespace AvaloniaTests.Converters
{
    public class AnswerStatusConverter : IValueConverter
    {
        public static AnswerStatusConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EqualityConverter : IValueConverter
    {
        public static EqualityConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (targetType == typeof(Avalonia.Media.Brush) || targetType == typeof(Avalonia.Media.IBrush))
            {
                bool isEqual = value.Equals(parameter);
                return isEqual ? 
                    new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#27AE60")) : 
                    new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#BDC3C7"));
            }

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}