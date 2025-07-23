using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Reactive;

namespace AvaloniaTests.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService _resultService;
        private readonly IErrorDialogService _errorDialogService;
        private readonly IWindowService _windowService;

        public ObservableCollection<Test> Tests { get; } = new();
        public ObservableCollection<TestResult> Results { get; } = new();

        public ICommand CreateTestCommand { get; }
        public ICommand EditTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand TakeTestCommand { get; }
        public ICommand ViewResultsCommand { get; }
        public ICommand OpenTestListCommand { get; }
        public ICommand StartTestCommand { get; }
        public ICommand OpenResultsTabCommand { get; }

        public MainWindowViewModel(ITestService testService, IResultService resultService, 
            IErrorDialogService errorDialogService, IWindowService windowService)
        {
            _testService = testService;
            _resultService = resultService;
            _errorDialogService = errorDialogService;
            _windowService = windowService;

            CreateTestCommand = ReactiveCommand.CreateFromTask(CreateTestAsync);
            EditTestCommand = ReactiveCommand.CreateFromTask<Test>(EditTestAsync);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            TakeTestCommand = ReactiveCommand.CreateFromTask<Test>(TakeTestAsync);
            ViewResultsCommand = ReactiveCommand.CreateFromTask<TestResult>(ViewResultAsync);
            OpenTestListCommand = ReactiveCommand.CreateFromTask(OpenTestListAsync);
            StartTestCommand = ReactiveCommand.CreateFromTask(OpenStartTestWindowAsync);
            OpenResultsTabCommand = ReactiveCommand.CreateFromTask(OpenResultsTabAsync);

            SubscribeToCommandErrors(CreateTestCommand);
            SubscribeToCommandErrors(EditTestCommand);
            SubscribeToCommandErrors(DeleteTestCommand);
            SubscribeToCommandErrors(TakeTestCommand);
            SubscribeToCommandErrors(ViewResultsCommand);
            SubscribeToCommandErrors(OpenTestListCommand);
            SubscribeToCommandErrors(StartTestCommand);
            SubscribeToCommandErrors(OpenResultsTabCommand);

            LoadData();
        }

        // Подписывает обработчик ошибок на исключения
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

        // Обработка исключений
        private void HandleCommandException(Exception ex)
        {
            _errorDialogService.ShowError("Ошибка", $"Произошла ошибка: {ex.Message}");
        }

        private void LoadData()
        {
            Tests.Clear();
            var tests = _testService.GetTests();
            
            foreach (var test in tests)
            {
                Tests.Add(test);
            }

            Results.Clear();
            var results = _resultService.GetResults();
            
            foreach (var result in results)
            {
                Results.Add(result);
            }
        }

        private async System.Threading.Tasks.Task CreateTestAsync()
        {
            var result = await _windowService.ShowTestEditorAsync();
            if (result)
            {
                LoadData();
            }
        }

        private async System.Threading.Tasks.Task EditTestAsync(Test test)
        {
            var result = await _windowService.ShowTestEditorAsync(test);
            if (result)
            {
                LoadData();
            }
        }

        private void DeleteTest(Test test)
        {
            _testService.DeleteTest(test.Id);
            LoadData();
        }

        private async System.Threading.Tasks.Task TakeTestAsync(Test test)
        {
            var result = await _windowService.ShowTestRunnerAsync(test);
            if (result)
            {
                LoadData();
            }
        }

        private async System.Threading.Tasks.Task ViewResultAsync(TestResult result)
        {
            var test = _testService.GetTests().FirstOrDefault(t => t.Id == result.TestId);
            await _windowService.ShowResultViewerAsync(result, test);
        }

        private async System.Threading.Tasks.Task OpenStartTestWindowAsync()
        {
            var result = await _windowService.ShowTestListAsync(true);
            if (result)
            {
                LoadData();
            }
        }

        private async System.Threading.Tasks.Task OpenTestListAsync()
        {
            var result = await _windowService.ShowTestListAsync(false);
            if (result)
            {
                LoadData();
            }
        }

        private async System.Threading.Tasks.Task OpenResultsTabAsync()
        {
            await _windowService.ShowResultsListAsync();
        }
    }
}