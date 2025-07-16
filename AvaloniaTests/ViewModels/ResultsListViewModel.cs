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
using Avalonia.Threading;

namespace AvaloniaTests.ViewModels
{
    public class ResultsListViewModel : ViewModelBase
    {
        private readonly IResultService _resultService;
        private readonly ITestService _testService;
        private readonly Window? _parentWindow;
        private Window? _currentWindow;
        private int _resultsCount;
        private bool _isLoading = false;

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

            (ViewResultCommand as ReactiveCommand<TestResultDisplayItem, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (DeleteResultCommand as ReactiveCommand<TestResultDisplayItem, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (CloseCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (RefreshCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);

            System.Diagnostics.Debug.WriteLine("ResultsListViewModel: Начинаем загрузку результатов в конструкторе");
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

        private void DeleteResult(TestResultDisplayItem item)
        {
            if (item?.Result != null)
            {
                try
                {
                    _resultService.DeleteResult(item.Result.Id);
                    Results.Remove(item);
                    ResultsCount = Results.Count;
                    System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.DeleteResult: Результат {item.Result.Id} удален");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.DeleteResult: Ошибка удаления результата: {ex}");
                    Console.WriteLine($"Ошибка удаления результата: {ex.Message}");
                }
            }
        }

        private void LoadResults()
        {
            if (_isLoading)
            {
                System.Diagnostics.Debug.WriteLine("ResultsListViewModel.LoadResults: Уже выполняется загрузка, пропускаем");
                return;
            }

            _isLoading = true;
            
            try
            {
                System.Diagnostics.Debug.WriteLine("ResultsListViewModel.LoadResults: Загружаем результаты");
                
                Results.Clear();
                System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: Очистили коллекцию Results, count = {Results.Count}");
                
                var results = _resultService.GetResults();
                var tests = _testService.GetTests();

                System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: Получено {results.Count} результатов из сервиса");
                System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: Получено {tests.Count} тестов из сервиса");

                if (results.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("ResultsListViewModel.LoadResults: ВНИМАНИЕ - результатов не найдено!");
                    ResultsCount = 0;
                    
                    // Принудительно уведомляем UI
                    Dispatcher.UIThread.Post(() =>
                    {
                        this.RaisePropertyChanged(nameof(Results));
                        this.RaisePropertyChanged(nameof(ResultsCount));
                    });
                    return;
                }

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
                    System.Diagnostics.Debug.WriteLine($"  - Добавлен результат: UserName='{displayItem.UserName}', TestTitle='{displayItem.TestTitle}', Score='{displayItem.Score}', CompletionDate='{displayItem.CompletionDate}', Percentage={displayItem.Percentage}");
                }

                ResultsCount = Results.Count;
                
                System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: ИТОГ загрузки - Results.Count = {Results.Count}, ResultsCount = {ResultsCount}");
                
                // Принудительно уведомляем UI об изменениях
                this.RaisePropertyChanged(nameof(Results));
                this.RaisePropertyChanged(nameof(ResultsCount));
                
                System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: Уведомили UI, финальная проверка - Results.Count = {Results.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListViewModel.LoadResults: ОШИБКА при загрузке результатов: {ex}");
                Console.WriteLine($"Ошибка загрузки результатов: {ex.Message}");
                
                Results.Clear();
                ResultsCount = 0;
                
                // Уведомляем об ошибке
                this.RaisePropertyChanged(nameof(Results));
                this.RaisePropertyChanged(nameof(ResultsCount));
            }
            finally
            {
                _isLoading = false;
            }
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