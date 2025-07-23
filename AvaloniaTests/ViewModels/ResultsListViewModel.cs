using AvaloniaTests.Services;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Controls;

namespace AvaloniaTests.ViewModels
{
    public class ResultsListViewModel : ViewModelBase
    {
        private readonly IResultService _resultService;
        private readonly ITestService _testService;
        private readonly IWindowService _windowService;
        private readonly Window? _parentWindow;
        private Window? _currentWindow;
        private int _resultsCount;

        // Коллекция для отображения результатов
        public ObservableCollection<TestResultDisplayItem> Results { get; } = new();

        public int ResultsCount
        {
            get => _resultsCount;
            set => this.RaiseAndSetIfChanged(ref _resultsCount, value);
        }

        public bool HasNoResults => Results.Count == 0;

        public ICommand ViewResultCommand { get; }
        public ICommand DeleteResultCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand RefreshCommand { get; }

        public ResultsListViewModel(IResultService resultService, ITestService testService, Window? parentWindow = null)
        {
            _resultService = resultService;
            _testService = testService;
            _parentWindow = parentWindow;
            _windowService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IWindowService>(ServiceProvider.Instance);

            ViewResultCommand = ReactiveCommand.CreateFromTask<TestResultDisplayItem>(ViewResultAsync);
            DeleteResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(DeleteResult);
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            RefreshCommand = ReactiveCommand.Create(LoadResults);

            Results.CollectionChanged += (s, e) => 
            {
                this.RaisePropertyChanged(nameof(HasNoResults));
            };

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

        private async System.Threading.Tasks.Task ViewResultAsync(TestResultDisplayItem item)
        {
            if (item?.Result != null)
            {
                await _windowService.ShowResultViewerAsync(item.Result, item.Test);
            }
        }
    }
}