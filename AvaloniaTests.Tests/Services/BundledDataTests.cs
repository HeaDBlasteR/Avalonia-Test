using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace AvaloniaTests.Tests.Services
{
    // Validates the sample tests.json that ships next to the executable and seeds the first run.
    public class BundledDataTests
    {
        internal static List<Test> LoadBundledTests()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "tests.json");
            Assert.True(File.Exists(path), $"Bundled tests.json was not copied to the output folder: {path}");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var tests = JsonSerializer.Deserialize<List<Test>>(File.ReadAllText(path), options);

            Assert.NotNull(tests);
            return tests;
        }

        [Fact]
        public void BundledTests_AreComplete()
        {
            var tests = LoadBundledTests();

            Assert.NotEmpty(tests);
            foreach (var test in tests)
            {
                Assert.False(string.IsNullOrWhiteSpace(test.Title), "Every test needs a title");
                Assert.NotEmpty(test.QuestionsData);

                foreach (var question in test.QuestionsData)
                {
                    Assert.False(string.IsNullOrWhiteSpace(question.Text), $"Question without text in '{test.Title}'");
                    Assert.True(question.AnswersData.Count >= 2, $"'{question.Text}' needs at least two answers");
                    Assert.All(question.AnswersData, answer => Assert.False(string.IsNullOrWhiteSpace(answer.Text)));
                    Assert.Contains(question.AnswersData, answer => answer.Id == question.CorrectAnswerId);
                }
            }
        }

        [Fact]
        public void BundledTests_HaveUniqueIds()
        {
            var tests = LoadBundledTests();

            var ids = tests.Select(t => t.Id)
                .Concat(tests.SelectMany(t => t.QuestionsData).Select(q => q.Id))
                .Concat(tests.SelectMany(t => t.QuestionsData).SelectMany(q => q.AnswersData).Select(a => a.Id))
                .ToList();

            Assert.DoesNotContain(Guid.Empty, ids);
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }
    }
}
