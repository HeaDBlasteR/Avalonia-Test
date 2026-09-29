using AvaloniaTests.Models;
using AvaloniaTests.Tests.Fakes;
using AvaloniaTests.ViewModels;
using System.Collections.Generic;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class TestRunnerViewModelTests
    {
        private readonly Test _test = SampleData.CreateCapitalsTest();
        private readonly FakeResultService _resultService = new();
        private readonly FakeDialogService _dialogService = new();

        private TestRunnerViewModel CreateViewModel() => new(_test, _resultService, _dialogService, "student");

        private static void ChooseAnswer(TestRunnerViewModel viewModel, int answerIndex) =>
            viewModel.SelectAnswerCommand.Execute(viewModel.CurrentQuestion.Answers[answerIndex].Id);

        [Fact]
        public void StartsOnFirstQuestion()
        {
            var viewModel = CreateViewModel();

            Assert.Equal("Столицы", viewModel.TestTitle);
            Assert.Equal("Столица Франции?", viewModel.CurrentQuestion.Text);
            Assert.Equal(1, viewModel.QuestionNumber);
            Assert.Equal(3, viewModel.TotalQuestions);
            Assert.Null(viewModel.SelectedAnswer);
            Assert.False(viewModel.CanGoPrevious);
            Assert.True(viewModel.CanGoNext);
            Assert.True(viewModel.HasMultipleQuestions);
        }

        [Fact]
        public void NextAndPrevious_NavigateBetweenQuestions()
        {
            var viewModel = CreateViewModel();

            viewModel.NextQuestionCommand.Execute(null);
            viewModel.NextQuestionCommand.Execute(null);

            Assert.Equal(3, viewModel.QuestionNumber);
            Assert.False(viewModel.CanGoNext);
            Assert.True(viewModel.CanGoPrevious);

            viewModel.PreviousQuestionCommand.Execute(null);

            Assert.Equal(2, viewModel.QuestionNumber);
            Assert.Equal("Столица Японии?", viewModel.CurrentQuestion.Text);
        }

        [Fact]
        public void Navigation_StaysWithinQuestionBounds()
        {
            var viewModel = CreateViewModel();

            viewModel.PreviousQuestionCommand.Execute(null);
            Assert.Equal(1, viewModel.QuestionNumber);

            for (var i = 0; i < 5; i++)
            {
                viewModel.NextQuestionCommand.Execute(null);
            }
            Assert.Equal(3, viewModel.QuestionNumber);
        }

        [Fact]
        public void Navigation_NotifiesTheViewAboutTheNewQuestion()
        {
            var viewModel = CreateViewModel();
            var changed = new List<string?>();
            viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            viewModel.NextQuestionCommand.Execute(null);

            Assert.Contains(nameof(TestRunnerViewModel.CurrentQuestion), changed);
            Assert.Contains(nameof(TestRunnerViewModel.QuestionNumber), changed);
            Assert.Contains(nameof(TestRunnerViewModel.CanGoPrevious), changed);
        }

        [Fact]
        public void SelectedAnswer_IsRestoredWhenReturningToQuestion()
        {
            var viewModel = CreateViewModel();
            var paris = viewModel.CurrentQuestion.Answers[0].Id;
            ChooseAnswer(viewModel, 0);

            viewModel.NextQuestionCommand.Execute(null);
            Assert.Null(viewModel.SelectedAnswer);

            viewModel.PreviousQuestionCommand.Execute(null);
            Assert.Equal(paris, viewModel.SelectedAnswer);
        }

        [Fact]
        public void FinishTest_ScoresAnswersSavesResultAndCloses()
        {
            var viewModel = CreateViewModel();
            bool? closeResult = null;
            viewModel.CloseRequested += (_, result) => closeResult = result;

            ChooseAnswer(viewModel, 0);                 // Париж - correct
            viewModel.NextQuestionCommand.Execute(null);
            ChooseAnswer(viewModel, 0);                 // Пекин - wrong
            viewModel.FinishTestCommand.Execute(null);  // Италия left unanswered

            var result = Assert.Single(_resultService.Results);
            Assert.Equal(_test.Id, result.TestId);
            Assert.Equal("student", result.UserName);
            Assert.Equal(1, result.Score);
            Assert.Equal(3, result.MaxScore);
            Assert.Equal(2, result.UserAnswers.Count);
            Assert.Same(result, Assert.Single(_dialogService.CompletionDialogs));
            Assert.True(closeResult);
        }

        [Fact]
        public void FinishTest_AllAnswersCorrect_GivesFullScore()
        {
            var viewModel = CreateViewModel();

            for (var i = 0; i < viewModel.TotalQuestions; i++)
            {
                viewModel.SelectAnswerCommand.Execute(viewModel.CurrentQuestion.CorrectAnswerId);
                viewModel.NextQuestionCommand.Execute(null);
            }
            viewModel.FinishTestCommand.Execute(null);

            var result = Assert.Single(_resultService.Results);
            Assert.Equal(result.MaxScore, result.Score);
        }
    }
}
