using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.Tests.Fakes;
using AvaloniaTests.Tests.Services;
using AvaloniaTests.ViewModels;
using System.Linq;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class TestEditorViewModelTests
    {
        private readonly FakeTestService _testService = new();
        private readonly FakeDialogService _dialogService = new();

        private TestEditorViewModel CreateViewModel(Test? test = null) => new(_testService, _dialogService, test);

        // Simulates the question editor window: edits a copy of the question and saves it.
        private Question EditInQuestionEditor(Question question, params string[] answers)
        {
            var editor = new QuestionEditorViewModel(question);
            while (editor.EditingQuestion.Answers.Count < answers.Length)
            {
                editor.AddAnswerCommand.Execute(null);
            }
            for (var i = 0; i < answers.Length; i++)
            {
                editor.EditingQuestion.Answers[i].Text = answers[i];
            }
            editor.SaveCommand.Execute(null);
            return editor.EditingQuestion;
        }

        [Fact]
        public void NewTest_CanBeSavedOnlyWithTitleAndAtLeastOneQuestion()
        {
            var viewModel = CreateViewModel();
            Assert.False(viewModel.CanSaveTest);

            viewModel.EditingTest.Title = "Новый тест";
            Assert.False(viewModel.CanSaveTest);

            _dialogService.QuestionToReturn = SampleData.CreateQuestion("2 + 2 = ?", 0, "4", "5");
            viewModel.AddQuestionCommand.Execute(null);

            Assert.True(viewModel.CanSaveTest);
            Assert.True(viewModel.SaveCommand.CanExecute(null));
        }

        [Fact]
        public void AddQuestion_WhenEditorIsCancelled_AddsNothing()
        {
            var viewModel = CreateViewModel();
            _dialogService.QuestionToReturn = null;

            viewModel.AddQuestionCommand.Execute(null);

            Assert.Null(Assert.Single(_dialogService.QuestionEditorRequests));
            Assert.Empty(viewModel.EditingTest.Questions);
        }

        [Fact]
        public void ExistingTest_IsEditedAsACopyWithTheSameIds()
        {
            var test = SampleData.CreateCapitalsTest();

            var editing = CreateViewModel(test).EditingTest;

            Assert.NotSame(test, editing);
            Assert.Equal(test.Id, editing.Id);
            Assert.Equal(test.Title, editing.Title);
            Assert.Equal(test.Questions.Select(q => q.Id), editing.Questions.Select(q => q.Id));
            Assert.Equal(test.Questions.Select(q => q.CorrectAnswerId), editing.Questions.Select(q => q.CorrectAnswerId));
            Assert.Equal(
                test.Questions.SelectMany(q => q.Answers).Select(a => (a.Id, a.Text)),
                editing.Questions.SelectMany(q => q.Answers).Select(a => (a.Id, a.Text)));
        }

        [Fact]
        public void Cancel_LeavesTheOriginalTestUnchanged()
        {
            var test = SampleData.CreateCapitalsTest();
            var viewModel = CreateViewModel(test);

            viewModel.EditingTest.Title = "Изменённое название";
            viewModel.EditingTest.Description = "Изменённое описание";
            viewModel.RemoveQuestionCommand.Execute(viewModel.EditingTest.Questions[2]);
            var france = viewModel.EditingTest.Questions[0];
            _dialogService.QuestionToReturn = EditInQuestionEditor(france, "Париж", "Берлин");
            viewModel.EditQuestionCommand.Execute(france);
            // The window is closed without saving.

            Assert.Equal("Столицы", test.Title);
            Assert.Equal("Описание: Столицы", test.Description);
            Assert.Equal(3, test.Questions.Count);
            Assert.Equal(new[] { "Париж", "Лондон" }, test.Questions[0].Answers.Select(a => a.Text));
            Assert.Empty(_testService.SavedTests);
        }

        [Fact]
        public void Save_PersistsTestAndClosesWithTrue()
        {
            var test = SampleData.CreateCapitalsTest();
            var viewModel = CreateViewModel(test);
            viewModel.EditingTest.Title = "Столицы мира";
            bool? closeResult = null;
            viewModel.CloseRequested += (_, result) => closeResult = result;

            viewModel.SaveCommand.Execute(null);

            var saved = Assert.Single(_testService.SavedTests);
            Assert.Equal(test.Id, saved.Id);
            Assert.Equal("Столицы мира", saved.Title);
            Assert.Equal(saved.Questions, saved.QuestionsData);
            Assert.True(closeResult);
        }

        [Fact]
        public void Save_TestWithoutQuestions_DoesNothing()
        {
            var viewModel = CreateViewModel(new Test("Без вопросов", ""));
            var closed = false;
            viewModel.CloseRequested += (_, _) => closed = true;

            viewModel.SaveCommand.Execute(null);

            Assert.Empty(_testService.SavedTests);
            Assert.False(closed);
        }

        [Fact]
        public void Save_AfterEditingAnswers_KeepsTheEditedAnswers()
        {
            var viewModel = CreateViewModel(SampleData.CreateCapitalsTest());
            var france = viewModel.EditingTest.Questions[0];
            _dialogService.QuestionToReturn = EditInQuestionEditor(france, "Париж", "Берлин", "Мадрид");
            viewModel.EditQuestionCommand.Execute(france);

            viewModel.SaveCommand.Execute(null);

            var savedQuestion = Assert.Single(_testService.SavedTests).Questions[0];
            Assert.Equal(new[] { "Париж", "Берлин", "Мадрид" }, savedQuestion.Answers.Select(a => a.Text));
            Assert.Equal(savedQuestion.Answers, savedQuestion.AnswersData);
            Assert.Contains(savedQuestion.Answers, a => a.Id == savedQuestion.CorrectAnswerId);
        }

        [Fact]
        public void Save_AfterAddingAndRemovingAnswers_KeepsTheChanges()
        {
            var viewModel = CreateViewModel(SampleData.CreateCapitalsTest());
            var japan = viewModel.EditingTest.Questions[1];
            viewModel.RemoveAnswerCommand.Execute(japan.Answers[2]);   // Сеул
            viewModel.AddAnswerCommand.Execute(japan);

            viewModel.SaveCommand.Execute(null);

            var savedQuestion = Assert.Single(_testService.SavedTests).Questions[1];
            Assert.Equal(new[] { "Пекин", "Токио", "" }, savedQuestion.Answers.Select(a => a.Text));
        }

        [Fact]
        public void EditAndSave_WithJsonStorage_PersistsChangesAndReplacesTheTest()
        {
            using var data = new TempDataDirectory();
            data.WriteFile("tests.json", "[]");
            var storage = new JsonTestService(data.DirectoryPath);
            storage.SaveTest(SampleData.CreateCapitalsTest());
            var stored = Assert.Single(storage.GetTests());

            var viewModel = new TestEditorViewModel(storage, _dialogService, stored);
            viewModel.EditingTest.Title = "Столицы мира";
            var france = viewModel.EditingTest.Questions[0];
            _dialogService.QuestionToReturn = EditInQuestionEditor(france, "Париж", "Берлин", "Мадрид");
            viewModel.EditQuestionCommand.Execute(france);
            viewModel.SaveCommand.Execute(null);

            var reloaded = Assert.Single(new JsonTestService(data.DirectoryPath).GetTests());
            Assert.Equal(stored.Id, reloaded.Id);
            Assert.Equal("Столицы мира", reloaded.Title);
            Assert.Equal(new[] { "Париж", "Берлин", "Мадрид" }, reloaded.Questions[0].Answers.Select(a => a.Text));
        }

        [Fact]
        public void RemoveQuestion_RemovesItFromTheTest()
        {
            var viewModel = CreateViewModel(SampleData.CreateCapitalsTest());
            var removed = viewModel.EditingTest.Questions[0];

            viewModel.RemoveQuestionCommand.Execute(removed);

            Assert.DoesNotContain(removed, viewModel.EditingTest.Questions);
            Assert.Equal(2, viewModel.EditingTest.Questions.Count);
        }

        [Fact]
        public void EditQuestion_AppliesChangesFromTheQuestionEditor()
        {
            var viewModel = CreateViewModel(SampleData.CreateCapitalsTest());
            var question = viewModel.EditingTest.Questions[0];
            var edited = SampleData.CreateQuestion("Столица Германии?", 1, "Мюнхен", "Берлин");
            _dialogService.QuestionToReturn = edited;

            viewModel.EditQuestionCommand.Execute(question);

            Assert.Same(question, Assert.Single(_dialogService.QuestionEditorRequests));
            Assert.Equal("Столица Германии?", question.Text);
            Assert.Equal(new[] { "Мюнхен", "Берлин" }, question.Answers.Select(a => a.Text));
            Assert.Equal(edited.Answers[1].Id, question.CorrectAnswerId);
        }

        [Fact]
        public void SetCorrectAnswer_UpdatesTheQuestion()
        {
            var viewModel = CreateViewModel(SampleData.CreateCapitalsTest());
            var question = viewModel.EditingTest.Questions[0];

            viewModel.SetCorrectAnswerCommand.Execute(new object[] { question, question.Answers[1] });

            Assert.Equal(question.Answers[1].Id, question.CorrectAnswerId);
        }

        [Fact]
        public void RemoveAnswer_MovesCorrectAnswerToFirstRemainingAnswer()
        {
            var viewModel = CreateViewModel(SampleData.CreateCapitalsTest());
            var japan = viewModel.EditingTest.Questions[1];
            var tokyo = japan.Answers[1];

            viewModel.RemoveAnswerCommand.Execute(tokyo);

            Assert.DoesNotContain(tokyo, japan.Answers);
            Assert.Equal(japan.Answers[0].Id, japan.CorrectAnswerId);
        }
    }
}
