using AvaloniaTests.Services;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Controls;
using System;
using System.Reactive;

namespace AvaloniaTests.ViewModels
{
    public class ResultsListViewModel : ViewModelBase
    {
        private readonly IResultService _resultService;
        private readonly ITestService _testService;
        private readonly IWindowService _windowService;
        private readonly IErrorDialogService _errorDialogService;
        private readonly Window? _parentWindow;
        private Window? _currentWindow;
        private int _resultsCount;

        // Коллекция отображаемых результатов
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
            _errorDialogService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IErrorDialogService>(ServiceProvider.Instance);

            ViewResultCommand = ReactiveCommand.CreateFromTask<TestResultDisplayItem>(ViewResultAsync);
            DeleteResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(DeleteResult);
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            RefreshCommand = ReactiveCommand.Create(LoadResults);

            SubscribeToCommandErrors(ViewResultCommand);
            SubscribeToCommandErrors(DeleteResultCommand);
            SubscribeToCommandErrors(CloseCommand);
            SubscribeToCommandErrors(RefreshCommand);

            Results.CollectionChanged += (s, e) => 
            {
                this.RaisePropertyChanged(nameof(HasNoResults));
            };

            LoadResults();
        }

        private void SubscribeToCommandErrors(ICommand command)
        {
            if (command is ReactiveCommand<Unit, Unit> reactiveCommand)
            {
                reactiveCommand.ThrownExceptions.Subscribe(HandleCommandException);
            }
            else if (command is IReactiveCommand reactiveCommandGeneric)
            {
                reactiveCommandGeneric.ThrownExceptions.Subscribe(HandleCommandException);
            }
        }

        private void HandleCommandException(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в команде ResultsList: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

            try
            {
                _errorDialogService.ShowError("Ошибка", $"Произошла ошибка: {ex.Message}");
            }
            catch
            {
            }
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private void CloseWindow()
        {
            try
            {
                _currentWindow?.Close();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void DeleteResult(TestResultDisplayItem item)
        {
            try
            {
                if (item?.Result != null)
                {
                    _resultService.DeleteResult(item.Result.Id);
                    Results.Remove(item);
                    ResultsCount = Results.Count;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void LoadResults()
        {
            try
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
            catch (Exception ex)
            {
                throw;
            }
        }

        private async System.Threading.Tasks.Task ViewResultAsync(TestResultDisplayItem item)
        {
            try
            {
                if (item?.Result != null)
                {
                    await _windowService.ShowResultViewerAsync(item.Result, item.Test);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}