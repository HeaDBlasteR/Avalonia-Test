using AvaloniaTests.Models;
using AvaloniaTests.Services;
using System;
using Xunit;

namespace AvaloniaTests.Tests.Services
{
    public sealed class JsonResultServiceTests : IDisposable
    {
        private const string ResultsFileName = "results.json";
        private readonly TempDataDirectory _data = new();

        public void Dispose() => _data.Dispose();

        private JsonResultService CreateService(string fileContent = "[]")
        {
            _data.WriteFile(ResultsFileName, fileContent);
            return new JsonResultService(_data.DirectoryPath);
        }

        private JsonResultService ReloadService() => new(_data.DirectoryPath);

        [Fact]
        public void SaveResult_PersistsResultIncludingUserAnswers()
        {
            var service = CreateService();
            var questionId = Guid.NewGuid();
            var answerId = Guid.NewGuid();
            var result = new TestResult
            {
                TestId = Guid.NewGuid(),
                UserName = "Иван",
                CompletionDate = new DateTime(2025, 3, 5, 14, 7, 0),
                Score = 2,
                MaxScore = 3,
                UserAnswers = { [questionId] = answerId }
            };

            service.SaveResult(result);

            var saved = Assert.Single(ReloadService().GetResults());
            Assert.Equal(result.Id, saved.Id);
            Assert.Equal(result.TestId, saved.TestId);
            Assert.Equal("Иван", saved.UserName);
            Assert.Equal(result.CompletionDate, saved.CompletionDate);
            Assert.Equal(2, saved.Score);
            Assert.Equal(3, saved.MaxScore);
            Assert.Equal(answerId, saved.UserAnswers[questionId]);
        }

        [Fact]
        public void SaveResult_AssignsIdWhenMissing()
        {
            var service = CreateService();
            var result = new TestResult { Id = Guid.Empty };

            service.SaveResult(result);

            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public void DeleteResult_RemovesResultFromStorage()
        {
            var service = CreateService();
            var kept = new TestResult { UserName = "kept" };
            var deleted = new TestResult { UserName = "deleted" };
            service.SaveResult(kept);
            service.SaveResult(deleted);

            service.DeleteResult(deleted.Id);

            var remaining = Assert.Single(ReloadService().GetResults());
            Assert.Equal(kept.Id, remaining.Id);
        }

        [Fact]
        public void Load_ReadsExistingFileWithCaseInsensitivePropertyNames()
        {
            var service = CreateService("""
                [
                  {
                    "id": "8bef5fac-d3cc-4ed7-928c-f0a13ca831dc",
                    "testId": "550e8400-e29b-41d4-a716-446655440000",
                    "userName": "student",
                    "completionDate": "2025-07-16T10:17:58",
                    "score": 1,
                    "maxScore": 1,
                    "userAnswers": { "550e8400-e29b-41d4-a716-446655440001": "550e8400-e29b-41d4-a716-446655440002" }
                  }
                ]
                """);

            var result = Assert.Single(service.GetResults());
            Assert.Equal(Guid.Parse("8bef5fac-d3cc-4ed7-928c-f0a13ca831dc"), result.Id);
            Assert.Equal("student", result.UserName);
            Assert.Equal(1, result.Score);
            Assert.Equal(
                Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                result.UserAnswers[Guid.Parse("550e8400-e29b-41d4-a716-446655440001")]);
        }

        [Fact]
        public void Load_EmptyFile_ReturnsNoResults()
        {
            var service = CreateService("   ");

            Assert.Empty(service.GetResults());
        }

        [Fact]
        public void GetResults_ReturnsCopyOfTheList()
        {
            var service = CreateService();
            service.SaveResult(new TestResult());

            service.GetResults().Clear();

            Assert.Single(service.GetResults());
        }
    }
}
