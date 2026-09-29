using AvaloniaTests.Models;
using AvaloniaTests.Services;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace AvaloniaTests.Tests.Services
{
    public sealed class JsonTestServiceTests : IDisposable
    {
        private const string TestsFileName = "tests.json";
        private readonly TempDataDirectory _data = new();

        public void Dispose() => _data.Dispose();

        private JsonTestService CreateService(string fileContent = "[]")
        {
            _data.WriteFile(TestsFileName, fileContent);
            return new JsonTestService(_data.DirectoryPath);
        }

        private JsonTestService ReloadService() => new(_data.DirectoryPath);

        [Fact]
        public void Constructor_WithoutDataFile_SeedsTestsBundledWithTheApp()
        {
            var service = new JsonTestService(_data.DirectoryPath);

            Assert.True(File.Exists(_data.FilePath(TestsFileName)));
            var bundled = BundledDataTests.LoadBundledTests();
            Assert.Equal(bundled.Select(t => t.Id), service.GetTests().Select(t => t.Id));
        }

        [Fact]
        public void SaveTest_PersistsTestSoANewInstanceLoadsIt()
        {
            var service = CreateService();
            var test = SampleData.CreateCapitalsTest();

            service.SaveTest(test);
            var reloaded = Assert.Single(ReloadService().GetTests());

            Assert.Equal(test.Id, reloaded.Id);
            Assert.Equal("Столицы", reloaded.Title);
            Assert.Equal(test.Description, reloaded.Description);
            Assert.Equal(test.Questions.Select(q => q.Id), reloaded.Questions.Select(q => q.Id));
            var japan = reloaded.Questions[1];
            Assert.Equal(new[] { "Пекин", "Токио", "Сеул" }, japan.Answers.Select(a => a.Text));
            Assert.Equal(japan.Answers[1].Id, japan.CorrectAnswerId);
        }

        [Fact]
        public void SaveTest_WritesCyrillicTextWithoutEscaping()
        {
            var service = CreateService();

            service.SaveTest(SampleData.CreateCapitalsTest());

            Assert.Contains("Столица Франции?", File.ReadAllText(_data.FilePath(TestsFileName)));
        }

        [Fact]
        public void SaveTest_ForExistingTest_ReplacesItInsteadOfAddingDuplicate()
        {
            var service = CreateService();
            var test = SampleData.CreateCapitalsTest();
            service.SaveTest(test);

            test.Title = "Столицы Европы";
            service.SaveTest(test);

            var saved = Assert.Single(ReloadService().GetTests());
            Assert.Equal("Столицы Европы", saved.Title);
        }

        [Fact]
        public void SaveTest_PersistsChangesMadeThroughObservableCollections()
        {
            var service = CreateService();
            var test = SampleData.CreateCapitalsTest();
            service.SaveTest(test);

            test.Questions.RemoveAt(0);
            test.Questions[0].Answers.Add(new Answer("Осака"));
            service.SaveTest(test);

            var saved = Assert.Single(ReloadService().GetTests());
            Assert.Equal(new[] { "Столица Японии?", "Столица Италии?" }, saved.Questions.Select(q => q.Text));
            Assert.Contains(saved.Questions[0].Answers, a => a.Text == "Осака");
        }

        [Fact]
        public void DeleteTest_RemovesTestFromStorage()
        {
            var service = CreateService();
            var first = SampleData.CreateCapitalsTest();
            var second = SampleData.CreateTest("Арифметика", SampleData.CreateQuestion("2 + 2 = ?", 0, "4", "5"));
            service.SaveTest(first);
            service.SaveTest(second);

            service.DeleteTest(first.Id);

            var remaining = Assert.Single(ReloadService().GetTests());
            Assert.Equal(second.Id, remaining.Id);
        }

        [Fact]
        public void DeleteTest_WithUnknownId_KeepsExistingTests()
        {
            var service = CreateService();
            service.SaveTest(SampleData.CreateCapitalsTest());

            service.DeleteTest(Guid.NewGuid());

            Assert.Single(service.GetTests());
        }

        [Fact]
        public void Load_SkipsTestsWithoutTitle()
        {
            var service = CreateService("""
                [
                  { "Id": "11111111-1111-1111-1111-111111111111", "Title": "С названием", "Description": "", "Questions": [] },
                  { "Id": "22222222-2222-2222-2222-222222222222", "Title": "   ", "Description": "", "Questions": [] }
                ]
                """);

            var test = Assert.Single(service.GetTests());
            Assert.Equal("С названием", test.Title);
        }

        [Fact]
        public void Load_EmptyFile_ReturnsNoTests()
        {
            var service = CreateService("   ");

            Assert.Empty(service.GetTests());
        }

        [Fact]
        public void GetTests_ReturnsCopyOfTheList()
        {
            var service = CreateService();
            service.SaveTest(SampleData.CreateCapitalsTest());

            service.GetTests().Clear();

            Assert.Single(service.GetTests());
        }
    }
}
