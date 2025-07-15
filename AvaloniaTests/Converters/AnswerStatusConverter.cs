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
}