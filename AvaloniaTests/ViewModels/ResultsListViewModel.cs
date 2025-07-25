using AvaloniaTests.Services;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Windows.Input;
using System;

namespace AvaloniaTests.ViewModels
{
    public class ResultsListViewModel : ViewModelBase
    {
        private readonly IResultService _resultService;
        private readonly ITestService _testService;
        private readonly IWindowService _windowService;
        private int _resultsCount;

        // Коллекция для отображения результатов
        public ObservableCollection<TestResultDisplayItem> Results { get; } = new();

        public int ResultsCount
        {
            get => _resultsCount;
            set => this.RaiseAndSetIfChanged(ref _resultsCount, value);
        }

        public bool HasNoResults => Results.Count == 0;

        public ICommand ViewResultCommand { get; private set; }
        public ICommand DeleteResultCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        // Событие для запроса закрытия окна
        public event EventHandler? CloseRequested;

        public ResultsListViewModel(IResultService resultService, ITestService testService)
        {
            _resultService = resultService;
            _testService = testService;
            _windowService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IWindowService>(ServiceProvider.Instance);

            InitializeCommands();

            Results.CollectionChanged += (s, e) => 
            {
                this.RaisePropertyChanged(nameof(HasNoResults));
            };

            LoadResults();
        }

        private void InitializeCommands()
        {
            ViewResultCommand = ReactiveCommand.CreateFromTask<TestResultDisplayItem>(ViewResultAsync);
            DeleteResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(DeleteResult);
            CloseCommand = ReactiveCommand.Create(RequestClose);
            RefreshCommand = ReactiveCommand.Create(LoadResults);
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
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