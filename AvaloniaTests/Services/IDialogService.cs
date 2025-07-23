using AvaloniaTests.Models;
using System.Threading.Tasks;

namespace AvaloniaTests.Services
{
    public interface IDialogService
    {
        Task<Question?> ShowQuestionEditorAsync(Question? question = null);

        Task ShowTestCompletionDialogAsync(TestResult result);

        Task<bool> ShowConfirmationAsync(string title, string message);
    }
}