using AvaloniaTests.Models;

namespace AvaloniaTests.ViewModels
{
    public class ResultViewModel : ViewModelBase
    {
        public TestResult Result { get; }
        public Test Test { get; }

        public ResultViewModel(TestResult result, Test test)
        {
            Result = result;
            Test = test;
        }
    }
}