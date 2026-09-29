using AvaloniaTests.Models;
using AvaloniaTests.Services;
using System;
using System.Collections.Generic;

namespace AvaloniaTests.Tests.Fakes
{
    internal sealed class FakeTestService : ITestService
    {
        public List<Test> Tests { get; } = new();
        public List<Test> SavedTests { get; } = new();
        public List<Guid> DeletedIds { get; } = new();

        public FakeTestService(params Test[] tests)
        {
            Tests.AddRange(tests);
        }

        public List<Test> GetTests() => new(Tests);

        public void SaveTest(Test test)
        {
            SavedTests.Add(test);
            Tests.RemoveAll(t => t.Id == test.Id);
            Tests.Add(test);
        }

        public void DeleteTest(Guid testId)
        {
            DeletedIds.Add(testId);
            Tests.RemoveAll(t => t.Id == testId);
        }
    }
}
