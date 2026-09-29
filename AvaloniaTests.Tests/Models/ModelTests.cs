using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using Xunit;

namespace AvaloniaTests.Tests.Models
{
    public class TestModelTests
    {
        [Fact]
        public void FixCollections_RebuildsQuestionsFromSerializedData()
        {
            var first = SampleData.CreateQuestion("Вопрос 1", 0, "А", "Б");
            var second = SampleData.CreateQuestion("Вопрос 2", 1, "В", "Г");
            var test = new Test("Тест", "Описание") { QuestionsData = new List<Question> { first, second } };

            test.FixCollections();

            Assert.Equal(new[] { first, second }, test.Questions);
            Assert.Equal(new[] { first, second }, test.QuestionsData);
        }

        [Fact]
        public void FixCollections_AssignsIdsToQuestionsAndAnswersWithoutIds()
        {
            var question = SampleData.CreateQuestion("Вопрос", 0, "А", "Б");
            question.Id = Guid.Empty;
            question.AnswersData[1].Id = Guid.Empty;
            var test = new Test("Тест", "") { QuestionsData = new List<Question> { question } };

            test.FixCollections();

            Assert.NotEqual(Guid.Empty, question.Id);
            Assert.All(question.Answers, answer => Assert.NotEqual(Guid.Empty, answer.Id));
        }

        [Fact]
        public void FixCollections_RaisesPropertyChangedForQuestions()
        {
            var test = SampleData.CreateCapitalsTest();
            var changed = new List<string?>();
            test.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            test.FixCollections();

            Assert.Contains(nameof(Test.Questions), changed);
        }

        [Fact]
        public void TitleAndDescription_RaisePropertyChanged()
        {
            var test = new Test("Старое название", "Старое описание");
            var changed = new List<string?>();
            test.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            test.Title = "Новое название";
            test.Description = "Новое описание";

            Assert.Equal(new[] { nameof(Test.Title), nameof(Test.Description) }, changed);
            Assert.Equal("Новое название", test.TestName);
        }
    }

    public class QuestionModelTests
    {
        [Fact]
        public void NewQuestion_HasIdAndNoCorrectAnswer()
        {
            var question = new Question("Вопрос");

            Assert.NotEqual(Guid.Empty, question.Id);
            Assert.Equal(Guid.Empty, question.CorrectAnswerId);
            Assert.Empty(question.Answers);
        }

        [Fact]
        public void FixCollections_RebuildsAnswersFromSerializedData()
        {
            var first = new Answer("А");
            var second = new Answer("Б");
            var question = new Question("Вопрос") { AnswersData = new List<Answer> { first, second } };

            question.FixCollections();

            Assert.Equal(new[] { first, second }, question.Answers);
        }

        [Fact]
        public void TextAndCorrectAnswerId_RaisePropertyChanged()
        {
            var question = new Question("Вопрос");
            var changed = new List<string?>();
            question.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            question.Text = "Другой вопрос";
            question.CorrectAnswerId = Guid.NewGuid();

            Assert.Equal(new[] { nameof(Question.Text), nameof(Question.CorrectAnswerId) }, changed);
        }
    }
}
