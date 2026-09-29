using AvaloniaTests.Models;
using AvaloniaTests.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaTests.Tests.Fakes
{
    internal sealed class FakeDialogService : IDialogService
    {
        // What the question editor "returns"; null means the user pressed Cancel.
        public Question? QuestionToReturn { get; set; }
        public List<Question?> QuestionEditorRequests { get; } = new();
        public List<TestResult> CompletionDialogs { get; } = new();

        public Task<Question?> ShowQuestionEditorAsync(Question? question = null)
        {
            QuestionEditorRequests.Add(question);
            return Task.FromResult(QuestionToReturn);
        }

        public Task ShowTestCompletionDialogAsync(TestResult result)
        {
            CompletionDialogs.Add(result);
            return Task.CompletedTask;
        }

        public Task<bool> ShowConfirmationAsync(string title, string message) => Task.FromResult(true);
    }
}
