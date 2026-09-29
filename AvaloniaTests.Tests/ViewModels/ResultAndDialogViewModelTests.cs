using AvaloniaTests.Models;
using AvaloniaTests.ViewModels;
using System;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class ResultViewModelTests
    {
        [Fact]
        public void FormatsResultForDisplay()
        {
            var test = SampleData.CreateCapitalsTest();
            var result = SampleData.CreateResult(test, 2, new DateTime(2025, 3, 5, 14, 7, 0), "Иван");

            var viewModel = new ResultViewModel(result, test);

            Assert.Equal("Столицы", viewModel.TestTitle);
            Assert.Equal(test.Description, viewModel.TestDescription);
            Assert.Equal("Иван", viewModel.UserName);
            Assert.Equal("2/3", viewModel.Score);
            Assert.Equal("05.03.2025 14:07", viewModel.CompletionDate);
            Assert.Equal(66, viewModel.Percentage); // truncated, not rounded
        }

        [Fact]
        public void UnknownTest_UsesPlaceholders()
        {
            var viewModel = new ResultViewModel(new TestResult { Score = 0, MaxScore = 0 }, null);

            Assert.Equal("Неизвестный тест", viewModel.TestTitle);
            Assert.Equal("Описание недоступно", viewModel.TestDescription);
            Assert.Equal(0, viewModel.Percentage);
        }
    }

    public class DialogViewModelTests
    {
        [Fact]
        public void TestCompletionDialog_ShowsScoreAndPercentage()
        {
            var viewModel = new TestCompletionDialogViewModel(new TestResult { Score = 2, MaxScore = 3 });

            Assert.Equal("Тест завершен!", viewModel.TestCompletedMessage);
            Assert.Equal("Ваш результат: 2 из 3", viewModel.ScoreMessage);
            Assert.Equal("Процент: 66%", viewModel.PercentageMessage);
        }

        [Fact]
        public void TestCompletionDialog_Ok_RequestsClose()
        {
            var viewModel = new TestCompletionDialogViewModel(new TestResult());
            var closed = false;
            viewModel.CloseRequested += (_, _) => closed = true;

            viewModel.OkCommand.Execute(null);

            Assert.True(closed);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConfirmationDialog_ReportsTheUsersChoice(bool confirm)
        {
            var viewModel = new ConfirmationDialogViewModel("Удаление", "Удалить тест?");
            bool? choice = null;
            viewModel.CloseRequested += (_, result) => choice = result;

            (confirm ? viewModel.YesCommand : viewModel.NoCommand).Execute(null);

            Assert.Equal(confirm, choice);
            Assert.Equal("Удаление", viewModel.Title);
            Assert.Equal("Удалить тест?", viewModel.Message);
        }

        [Fact]
        public void ErrorDialog_Ok_RequestsClose()
        {
            var viewModel = new ErrorDialogViewModel("Ошибка", "Не удалось сохранить тест");
            var closed = false;
            viewModel.CloseRequested += (_, _) => closed = true;

            viewModel.OkCommand.Execute(null);

            Assert.True(closed);
            Assert.Equal("Ошибка", viewModel.Title);
            Assert.Equal("Не удалось сохранить тест", viewModel.Message);
        }
    }
}
