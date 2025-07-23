using AvaloniaTests.Models;
using ReactiveUI;

namespace AvaloniaTests.ViewModels
{
    public class TestResultDisplayItem : ViewModelBase
    {
        private TestResult? _result;
        private Test? _test;
        private string _testTitle = "";
        private string _userName = "";
        private string _score = "";
        private string _completionDate = "";
        private int _percentage;

        public TestResult? Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        public Test? Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }

        public string TestTitle
        {
            get => _testTitle;
            set => this.RaiseAndSetIfChanged(ref _testTitle, value);
        }

        public string UserName
        {
            get => _userName;
            set => this.RaiseAndSetIfChanged(ref _userName, value);
        }

        public string Score
        {
            get => _score;
            set => this.RaiseAndSetIfChanged(ref _score, value);
        }

        public string CompletionDate
        {
            get => _completionDate;
            set => this.RaiseAndSetIfChanged(ref _completionDate, value);
        }

        public int Percentage
        {
            get => _percentage;
            set => this.RaiseAndSetIfChanged(ref _percentage, value);
        }
    }
}