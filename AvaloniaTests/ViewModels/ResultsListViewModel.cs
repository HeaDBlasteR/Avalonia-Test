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

        public ObservableCollection<TestResultDisplayItem> Results { get; } = new();

        public ICommand ViewResultCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand RefreshCommand { get; }

        public ResultsListViewModel(IResultService resultService, ITestService testService, Window? parentWindow = null)
        {
            _resultService = resultService;
            _testService = testService;
            _parentWindow = parentWindow;

            ViewResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(ViewResult);
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            RefreshCommand = ReactiveCommand.Create(LoadResults);

            (ViewResultCommand as ReactiveCommand<TestResultDisplayItem, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (CloseCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (RefreshCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);

            LoadResults();
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private void HandleCommandException(Exception ex)
        {
            Console.WriteLine($"Ошибка команды в ResultsListViewModel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.HandleCommandException: {ex}");
        }

        private void CloseWindow()
        {
            _currentWindow?.Close();
        }

        private void LoadResults()
        {
            System.Diagnostics.Debug.WriteLine("ResultsListViewModel.LoadResults: Загружаем результаты");
            
            Results.Clear();
            var results = _resultService.GetResults();
            var tests = _testService.GetTests();

            System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: Найдено {results.Count} результатов");

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
                    Percentage = test != null && test.Questions.Count > 0 
                        ? (int)((double)result.Score / result.MaxScore * 100) 
                        : 0
                };
                Results.Add(displayItem);
                
                System.Diagnostics.Debug.WriteLine($"  - Добавлен результат: {displayItem.UserName} - {displayItem.TestTitle} - {displayItem.Score}");
            }

            System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: Загружено {Results.Count} результатов в список");
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

    public class TestResultDisplayItem
    {
        public TestResult? Result { get; set; }
        public Test? Test { get; set; }
        public string TestTitle { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Score { get; set; } = "";
        public string CompletionDate { get; set; } = "";
        public int Percentage { get; set; }
    }
}