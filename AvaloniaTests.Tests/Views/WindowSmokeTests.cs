using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaTests.Models;
using AvaloniaTests.Tests.Fakes;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AvaloniaTests.Tests.Views
{
    // Every window is loaded with the real App styles on the headless platform and bound to its view model.
    public class WindowSmokeTests
    {
        private static T Show<T>(T window, object dataContext) where T : Window
        {
            window.DataContext = dataContext;
            window.Show();
            Dispatcher.UIThread.RunJobs();
            return window;
        }

        private static List<string> TextsOf(Window window) =>
            window.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text ?? string.Empty).ToList();

        private static List<string> ShowAndCollectTexts(Window window, object dataContext)
        {
            Show(window, dataContext);
            var texts = TextsOf(window);
            window.Close();
            return texts;
        }

        [AvaloniaFact]
        public void MainWindow_BindsAllMenuButtons()
        {
            // Only constructed, never shown: closing the main window shuts the whole application down.
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(new FakeTestService(), new FakeResultService(),
                    new FakeErrorDialogService(), new FakeWindowService())
            };

            var buttons = window.GetLogicalDescendants().OfType<Button>().ToList();

            Assert.Equal("Система Тестирования", window.Title);
            Assert.Equal(4, buttons.Count);
            Assert.All(buttons, button => Assert.NotNull(button.Command));
        }

        [AvaloniaFact]
        public void TestListWindow_ShowsTests()
        {
            var test = SampleData.CreateCapitalsTest();

            var texts = ShowAndCollectTexts(new TestListWindow(),
                new TestListViewModel(new FakeTestService(test), new FakeWindowService()));

            Assert.Contains("Всего тестов: 1", texts);
            Assert.Contains("Столицы", texts);
            Assert.Contains("Количество вопросов: 3", texts);
        }

        [AvaloniaFact]
        public void TestEditorWindow_ShowsQuestionsOfTheTest()
        {
            var test = SampleData.CreateCapitalsTest();

            var texts = ShowAndCollectTexts(new TestEditorWindow(),
                new TestEditorViewModel(new FakeTestService(), new FakeDialogService(), test));

            Assert.Contains("Всего: 3", texts);
            Assert.Contains("Столица Франции?", texts);
            Assert.Contains("Токио", texts);
        }

        [AvaloniaFact]
        public void TestEditorWindow_HighlightsCorrectAnswers()
        {
            const string green = "#ff27ae60";
            const string gray = "#ffbdc3c7";
            var viewModel = new TestEditorViewModel(new FakeTestService(), new FakeDialogService(), SampleData.CreateCapitalsTest());
            var window = Show(new TestEditorWindow(), viewModel);

            List<string> MarkerColors() => window.GetVisualDescendants()
                .OfType<Border>()
                .Where(b => b.Width == 20 && b.Height == 20)
                .Select(b => (b.Background as ISolidColorBrush)?.Color.ToString() ?? "none")
                .ToList();

            Assert.Equal(new[] { green, gray, gray, green, gray, green, gray }, MarkerColors());

            var france = viewModel.EditingTest.Questions[0];
            viewModel.SetCorrectAnswerCommand.Execute(new object[] { france, france.Answers[1] });
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { gray, green, gray, green, gray, green, gray }, MarkerColors());
            window.Close();
        }

        [AvaloniaFact]
        public void QuestionEditorWindow_ShowsAnswersAndMarksTheCorrectOne()
        {
            var question = SampleData.CreateQuestion("Столица Японии?", 1, "Пекин", "Токио", "Сеул");
            var window = Show(new QuestionEditorWindow(), new QuestionEditorViewModel(question));

            var radioButtons = window.GetVisualDescendants().OfType<RadioButton>().ToList();

            Assert.Equal("Редактирование вопроса", window.Title);
            Assert.Equal(new bool?[] { false, true, false }, radioButtons.Select(r => r.IsChecked));
            window.Close();
        }

        [AvaloniaFact]
        public void TestRunnerWindow_ShowsCurrentQuestionAndItsAnswers()
        {
            var test = SampleData.CreateCapitalsTest();
            var window = Show(new TestRunnerWindow(),
                new TestRunnerViewModel(test, new FakeResultService(), new FakeDialogService(), "student"));

            var texts = TextsOf(window);
            var answers = window.GetVisualDescendants().OfType<RadioButton>().Select(r => r.Content as string);

            Assert.Contains("Вопрос 1 из 3", texts);
            Assert.Contains("Столица Франции?", texts);
            Assert.Equal(new[] { "Париж", "Лондон" }, answers);
            window.Close();
        }

        [AvaloniaFact]
        public void ResultsListWindow_ShowsResultRows()
        {
            var test = SampleData.CreateCapitalsTest();
            var result = SampleData.CreateResult(test, 2, new DateTime(2025, 3, 5, 14, 7, 0), "anna");

            var texts = ShowAndCollectTexts(new ResultsListWindow(),
                new ResultsListViewModel(new FakeResultService(result), new FakeTestService(test), new FakeWindowService()));

            Assert.Contains("Найдено результатов: 1", texts);
            Assert.Contains("anna", texts);
            Assert.Contains("Столицы", texts);
            Assert.Contains("2/3", texts);
            Assert.Contains("66%", texts);
        }

        [AvaloniaFact]
        public void ResultWindow_ShowsResultSummary()
        {
            var test = SampleData.CreateCapitalsTest();
            var result = SampleData.CreateResult(test, 2, new DateTime(2025, 3, 5, 14, 7, 0), "anna");

            var texts = ShowAndCollectTexts(new ResultWindow(), new ResultViewModel(result, test));

            Assert.Contains("Тест: Столицы", texts);
            Assert.Contains("Пользователь: anna", texts);
            Assert.Contains("Результат: 2/3", texts);
            Assert.Contains("Процент правильных ответов: 66%", texts);
        }

        [AvaloniaFact]
        public void TestCompletionDialogWindow_ShowsScore()
        {
            var texts = ShowAndCollectTexts(new TestCompletionDialogWindow(),
                new TestCompletionDialogViewModel(new TestResult { Score = 2, MaxScore = 3 }));

            Assert.Contains("Тест завершен!", texts);
            Assert.Contains("Ваш результат: 2 из 3", texts);
        }

        [AvaloniaFact]
        public void ConfirmationDialogWindow_ShowsTitleAndMessage()
        {
            var texts = ShowAndCollectTexts(new ConfirmationDialogWindow(),
                new ConfirmationDialogViewModel("Удаление", "Удалить тест?"));

            Assert.Contains("Удаление", texts);
            Assert.Contains("Удалить тест?", texts);
        }

        [AvaloniaFact]
        public void ErrorDialogWindow_ShowsTitleAndMessage()
        {
            var texts = ShowAndCollectTexts(new ErrorDialogWindow(),
                new ErrorDialogViewModel("Ошибка", "Не удалось сохранить тест"));

            Assert.Contains("Ошибка", texts);
            Assert.Contains("Не удалось сохранить тест", texts);
        }
    }
}
