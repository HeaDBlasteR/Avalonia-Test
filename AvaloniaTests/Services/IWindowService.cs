using AvaloniaTests.Models;
using System.Threading.Tasks;

namespace AvaloniaTests.Services
{
    public interface IWindowService
    {
        Task<bool> ShowTestEditorAsync(Test? testToEdit = null);

        Task<bool> ShowTestRunnerAsync(Test test);

        Task ShowResultViewerAsync(TestResult result, Test? test);

        Task<bool> ShowTestListAsync(bool selectMode = false);

        Task<bool> ShowResultsListAsync();

        Task<Question?> ShowQuestionEditorAsync(Question? question = null);

        void CloseCurrentWindow();
    }
}