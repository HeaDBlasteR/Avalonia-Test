using AvaloniaTests.Models;
using System.Collections.Generic;

namespace AvaloniaTests.Services
{
    public interface IResultService
    {
        List<TestResult> GetResults();
        void SaveResult(TestResult result);
    }
}
