using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.Views;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Windows.Input;
using System.Reactive;
using Avalonia.Controls;
using System;

namespace AvaloniaTests.ViewModels
{
    public class ResultsListViewModel : ViewModelBase
    {
        private readonly IResultService _resultService;
        private readonly ITestService _testService;
        private readonly Window? _parentWindow;
        private Window? _currentWindow;
        private int _resultsCount;

        public ObservableCollection<TestResultDisplayItem> Results { get; } = new();

        public int ResultsCount
        {
            get => _resultsCount;
            set => this.RaiseAndSetIfChanged(ref _resultsCount, value);
        }

        public ICommand ViewResultCommand { get; }
        public ICommand DeleteResultCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand RefreshCommand { get; }

        public ResultsListViewModel(IResultService resultService, ITestService testService, Window? parentWindow = null)
        {
            _resultService = resultService;
            _testService = testService;
            _parentWindow = parentWindow;

            ViewResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(ViewResult);
            DeleteResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(DeleteResult);
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            RefreshCommand = ReactiveCommand.Create(LoadResults);

            LoadResults();
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private void CloseWindow()
        {
            _currentWindow?.Close();
        }

        private void DeleteResult(TestResultDisplayItem item)
        {
            if (item?.Result != null)
            {
                _resultService.DeleteResult(item.Result.Id);
                Results.Remove(item);
                ResultsCount = Results.Count;
            }
        }

        private void LoadResults()
        {
            Results.Clear();
            
            var results = _resultService.GetResults();
            var tests = _testService.GetTests();

            foreach (var result in results.OrderByDescending(r => r.CompletionDate))
            {
                var test = tests.FirstOrDefault(t => t.Id == result.TestId);
                var displayItem = new TestResultDisplayItem
                {
                    Result = result,
                    Test = test,
                    TestTitle = test?.Title ?? "Неизвестный тест",
                    UserName = result.UserName,
                    Score = $"{result.Score}/{result.MaxScore}",
                    CompletionDate = result.CompletionDate.ToString("dd.MM.yyyy HH:mm"),
                    Percentage = result.MaxScore > 0 
                        ? (int)((double)result.Score / result.MaxScore * 100) 
                        : 0
                };
                
                Results.Add(displayItem);
            }

            ResultsCount = Results.Count;
        }

        private void ViewResult(TestResultDisplayItem item)
        {
            if (item?.Result != null && item?.Test != null)
            {
                var resultViewModel = new ResultViewModel(item.Result, item.Test);
                var resultWindow = new ResultWindow(resultViewModel);
                resultWindow.ShowDialog(_currentWindow ?? _parentWindow);
            }
        }
    }

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