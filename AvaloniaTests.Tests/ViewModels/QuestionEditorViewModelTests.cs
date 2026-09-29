using AvaloniaTests.ViewModels;
using System;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class QuestionEditorViewModelTests
    {
        private static void FillValidQuestion(QuestionEditorViewModel viewModel)
        {
            viewModel.EditingQuestion.Text = "2 + 2 = ?";
            viewModel.EditingQuestion.Answers[0].Text = "4";
            viewModel.EditingQuestion.Answers[1].Text = "5";
            viewModel.SetCorrectAnswerCommand.Execute(viewModel.EditingQuestion.Answers[0]);
        }

        [Fact]
        public void NewQuestion_StartsWithTwoEmptyAnswersAndCannotBeSaved()
        {
            var viewModel = new QuestionEditorViewModel();

            Assert.False(viewModel.IsEditMode);
            Assert.Equal("Добавление вопроса", viewModel.WindowTitle);
            Assert.Equal(2, viewModel.EditingQuestion.Answers.Count);
            Assert.False(viewModel.CanSaveQuestion);
            Assert.False(viewModel.CanRemoveAnswers);
            Assert.False(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void EditMode_ChangesACopyAndKeepsTheOriginalIntact()
        {
            var original = SampleData.CreateQuestion("Столица Франции?", 0, "Париж", "Лондон");

            var viewModel = new QuestionEditorViewModel(original);
            viewModel.EditingQuestion.Text = "Столица Германии?";
            viewModel.EditingQuestion.Answers[1].Text = "Берлин";

            Assert.True(viewModel.IsEditMode);
            Assert.Equal("Редактирование вопроса", viewModel.WindowTitle);
            Assert.NotSame(original, viewModel.EditingQuestion);
            Assert.Equal(original.Id, viewModel.EditingQuestion.Id);
            Assert.Equal(original.CorrectAnswerId, viewModel.EditingQuestion.CorrectAnswerId);
            Assert.Equal("Столица Франции?", original.Text);
            Assert.Equal("Лондон", original.Answers[1].Text);
        }

        [Fact]
        public void CanSaveQuestion_RequiresTextFilledAnswersAndCorrectAnswer()
        {
            var viewModel = new QuestionEditorViewModel();
            var question = viewModel.EditingQuestion;

            question.Text = "2 + 2 = ?";
            question.Answers[0].Text = "4";
            question.Answers[1].Text = "5";
            Assert.False(viewModel.CanSaveQuestion);

            viewModel.SetCorrectAnswerCommand.Execute(question.Answers[0]);
            Assert.True(viewModel.CanSaveQuestion);
            Assert.True(viewModel.SaveCommand.CanExecute(null));

            question.Answers[1].Text = " ";
            Assert.False(viewModel.CanSaveQuestion);
        }

        [Fact]
        public void AddAnswer_AllowsRemovingAnswersWhenThereAreMoreThanTwo()
        {
            var viewModel = new QuestionEditorViewModel();

            viewModel.AddAnswerCommand.Execute(null);

            Assert.Equal(3, viewModel.EditingQuestion.Answers.Count);
            Assert.True(viewModel.CanRemoveAnswers);
        }

        [Fact]
        public void RemoveAnswer_ResetsCorrectAnswerWhenItWasRemoved()
        {
            var viewModel = new QuestionEditorViewModel();
            viewModel.AddAnswerCommand.Execute(null);
            var answer = viewModel.EditingQuestion.Answers[2];
            viewModel.SetCorrectAnswerCommand.Execute(answer);

            viewModel.RemoveAnswerCommand.Execute(answer);

            Assert.Equal(2, viewModel.EditingQuestion.Answers.Count);
            Assert.Equal(Guid.Empty, viewModel.EditingQuestion.CorrectAnswerId);
        }

        [Fact]
        public void Save_ValidQuestion_ClosesWithTrueAndSyncsAnswers()
        {
            var viewModel = new QuestionEditorViewModel();
            FillValidQuestion(viewModel);
            bool? closeResult = null;
            viewModel.CloseRequested += (_, result) => closeResult = result;

            viewModel.SaveCommand.Execute(null);

            Assert.True(closeResult);
            Assert.Equal(viewModel.EditingQuestion.Answers, viewModel.EditingQuestion.AnswersData);
        }

        [Fact]
        public void Save_InvalidQuestion_DoesNotClose()
        {
            var viewModel = new QuestionEditorViewModel();
            var closed = false;
            viewModel.CloseRequested += (_, _) => closed = true;

            viewModel.SaveCommand.Execute(null);

            Assert.False(closed);
        }

        [Fact]
        public void Cancel_ClosesWithFalse()
        {
            var viewModel = new QuestionEditorViewModel();
            bool? closeResult = null;
            viewModel.CloseRequested += (_, result) => closeResult = result;

            viewModel.CancelCommand.Execute(null);

            Assert.False(closeResult);
        }
    }
}
