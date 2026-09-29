using AvaloniaTests.Models;
using AvaloniaTests.Services;
using System;
using System.Collections.Generic;

namespace AvaloniaTests.Tests.Fakes
{
    internal sealed class FakeResultService : IResultService
    {
        public List<TestResult> Results { get; } = new();

        public FakeResultService(params TestResult[] results)
        {
            Results.AddRange(results);
        }

        public List<TestResult> GetResults() => new(Results);

        public void SaveResult(TestResult result) => Results.Add(result);

        public void DeleteResult(Guid resultId) => Results.RemoveAll(r => r.Id == resultId);
    }
}
