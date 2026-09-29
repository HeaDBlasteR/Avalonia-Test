using AvaloniaTests.Models;
using AvaloniaTests.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaTests.Tests.Fakes
{
    internal sealed class FakeWindowService : IWindowService
    {
        // Values the "windows" return when they are closed.
        public bool TestEditorResult { get; set; }
        public bool TestRunnerResult { get; set; }

        public List<Test?> EditedTests { get; } = new();
        public List<Test> RunTests { get; } = new();
        public List<(TestResult Result, Test? Test)> ViewedResults { get; } = new();
        public List<bool> TestListRequests { get; } = new();
        public int ResultsListRequests { get; private set; }

        public Task<bool> ShowTestEditorAsync(Test? testToEdit = null)
        {
            EditedTests.Add(testToEdit);
            return Task.FromResult(TestEditorResult);
        }

        public Task<bool> ShowTestRunnerAsync(Test test)
        {
            RunTests.Add(test);
            return Task.FromResult(TestRunnerResult);
        }

        public Task ShowResultViewerAsync(TestResult result, Test? test)
        {
            ViewedResults.Add((result, test));
            return Task.CompletedTask;
        }

        public Task<bool> ShowTestListAsync(bool selectMode = false)
        {
            TestListRequests.Add(selectMode);
            return Task.FromResult(false);
        }

        public Task<bool> ShowResultsListAsync()
        {
            ResultsListRequests++;
            return Task.FromResult(false);
        }

        public Task<Question?> ShowQuestionEditorAsync(Question? question = null) => Task.FromResult<Question?>(null);

        public void CloseCurrentWindow()
        {
        }
    }
}
