using AvaloniaTests.Models;
using System;
using System.Linq;

namespace AvaloniaTests.Tests
{
    internal static class SampleData
    {
        public static Question CreateQuestion(string text, int correctAnswerIndex, params string[] answers)
        {
            var question = new Question(text);
            foreach (var answer in answers)
            {
                question.Answers.Add(new Answer(answer));
            }

            question.AnswersData = question.Answers.ToList();
            question.CorrectAnswerId = question.Answers[correctAnswerIndex].Id;
            return question;
        }

        public static Test CreateTest(string title, params Question[] questions)
        {
            var test = new Test(title, $"Описание: {title}");
            foreach (var question in questions)
            {
                test.Questions.Add(question);
            }

            test.QuestionsData = test.Questions.ToList();
            return test;
        }

        public static Test CreateCapitalsTest() => CreateTest("Столицы",
            CreateQuestion("Столица Франции?", 0, "Париж", "Лондон"),
            CreateQuestion("Столица Японии?", 1, "Пекин", "Токио", "Сеул"),
            CreateQuestion("Столица Италии?", 0, "Рим", "Милан"));

        public static TestResult CreateResult(Test test, int score, DateTime completionDate, string userName = "student") => new()
        {
            TestId = test.Id,
            UserName = userName,
            CompletionDate = completionDate,
            Score = score,
            MaxScore = test.Questions.Count
        };
    }
}
