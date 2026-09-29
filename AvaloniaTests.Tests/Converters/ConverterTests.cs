using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using AvaloniaTests.Converters;
using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace AvaloniaTests.Tests.Converters
{
    public class AnswerStatusConverterTests
    {
        private static object Convert(object? value, object? parameter) =>
            AnswerStatusConverter.Instance.Convert(value, typeof(string), parameter, CultureInfo.InvariantCulture);

        [Theory]
        [InlineData(0, "✅ Верно")]
        [InlineData(1, "❌ Неверно")]
        public void Convert_AnsweredQuestion_ShowsWhetherAnswerIsCorrect(int chosenAnswerIndex, string expected)
        {
            var question = SampleData.CreateQuestion("2 + 2 = ?", 0, "4", "5");
            var result = new TestResult();
            result.UserAnswers[question.Id] = question.Answers[chosenAnswerIndex].Id;

            Assert.Equal(expected, Convert(question, result));
        }

        [Fact]
        public void Convert_UnansweredQuestion_ShowsNotAnswered()
        {
            var question = SampleData.CreateQuestion("2 + 2 = ?", 0, "4", "5");

            Assert.Equal("Не отвечено", Convert(question, new TestResult()));
        }

        [Fact]
        public void Convert_UnexpectedInput_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, Convert("not a question", null));
        }
    }

    public class EqualityConverterTests
    {
        private static object Convert(object? value, Type targetType, object? parameter) =>
            EqualityConverter.Instance.Convert(value, targetType, parameter, CultureInfo.InvariantCulture);

        [Fact]
        public void Convert_ToBool_ComparesValues()
        {
            var id = Guid.NewGuid();

            Assert.True((bool)Convert(id, typeof(bool), id));
            Assert.False((bool)Convert(id, typeof(bool), Guid.NewGuid()));
        }

        [Fact]
        public void Convert_WithNullValueOrParameter_ReturnsFalse()
        {
            Assert.False((bool)Convert(null, typeof(bool), Guid.NewGuid()));
            Assert.False((bool)Convert(Guid.NewGuid(), typeof(bool), null));
        }

        // Brushes are Avalonia objects, so these run on the headless UI thread.
        [AvaloniaTheory]
        [InlineData(true, "#27AE60")]
        [InlineData(false, "#BDC3C7")]
        public void Convert_ToBrush_ReturnsGreenForEqualValuesAndGrayOtherwise(bool equal, string expectedColor)
        {
            var id = Guid.NewGuid();

            var result = Convert(id, typeof(IBrush), equal ? id : Guid.NewGuid());

            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(result);
            Assert.Equal(Color.Parse(expectedColor), brush.Color);
        }

        private static object MultiConvert(Type targetType, params object?[] values) =>
            EqualityConverter.Instance.Convert(new List<object?>(values), targetType, null, CultureInfo.InvariantCulture);

        [Fact]
        public void MultiConvert_ToBool_ComparesTheTwoBoundValues()
        {
            var id = Guid.NewGuid();

            Assert.True((bool)MultiConvert(typeof(bool), id, id));
            Assert.False((bool)MultiConvert(typeof(bool), id, Guid.NewGuid()));
            Assert.False((bool)MultiConvert(typeof(bool), null, null));
            Assert.False((bool)MultiConvert(typeof(bool), id));
        }

        [AvaloniaTheory]
        [InlineData(true, "#27AE60")]
        [InlineData(false, "#BDC3C7")]
        public void MultiConvert_ToBrush_ReturnsGreenForEqualValuesAndGrayOtherwise(bool equal, string expectedColor)
        {
            var id = Guid.NewGuid();

            var result = MultiConvert(typeof(IBrush), id, equal ? id : Guid.NewGuid());

            var brush = Assert.IsAssignableFrom<ISolidColorBrush>(result);
            Assert.Equal(Color.Parse(expectedColor), brush.Color);
        }
    }

    public class IsSelectedConverterTests
    {
        private static bool Convert(params object?[] values) =>
            (bool)IsSelectedConverter.Instance.Convert(new List<object?>(values), typeof(bool?), null, CultureInfo.InvariantCulture);

        [Fact]
        public void Convert_SameIds_ReturnsTrue()
        {
            var id = Guid.NewGuid();

            Assert.True(Convert(id, id));
        }

        [Fact]
        public void Convert_DifferentIds_ReturnsFalse()
        {
            Assert.False(Convert(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public void Convert_NothingSelected_ReturnsFalse()
        {
            Assert.False(Convert(Guid.NewGuid(), null));
        }

        [Fact]
        public void Convert_UnexpectedNumberOfValues_ReturnsFalse()
        {
            Assert.False(Convert(Guid.NewGuid()));
        }
    }

    public class ObjectEqualsConverterTests
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        [Fact]
        public void Convert_ReturnsWhetherValueEqualsParameter()
        {
            Assert.True((bool)ObjectEqualsConverter.Instance.Convert("A", typeof(bool), "A", Culture));
            Assert.False((bool)ObjectEqualsConverter.Instance.Convert("A", typeof(bool), "B", Culture));
        }

        [Fact]
        public void ConvertBack_Checked_ReturnsParameter()
        {
            Assert.Equal("A", ObjectEqualsConverter.Instance.ConvertBack(true, typeof(string), "A", Culture));
        }

        [Fact]
        public void ConvertBack_Unchecked_LeavesSourceUnchanged()
        {
            Assert.Same(BindingOperations.DoNothing, ObjectEqualsConverter.Instance.ConvertBack(false, typeof(string), "A", Culture));
        }
    }
}
