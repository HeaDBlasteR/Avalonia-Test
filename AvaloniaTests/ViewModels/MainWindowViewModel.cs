using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.Views;
using AvaloniaTests.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService _resultService;

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

        public MainWindowViewModel(ITestService testService, IResultService resultService)
        {
            _testService = testService;
            _resultService = resultService;

            CreateTestCommand = ReactiveCommand.Create(CreateTest);
            EditTestCommand = ReactiveCommand.Create<Test>(EditTest);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            TakeTestCommand = ReactiveCommand.Create<Test>(TakeTest);
            ViewResultsCommand = ReactiveCommand.Create<TestResult>(ViewResult);
            OpenTestListCommand = ReactiveCommand.Create(OpenTestList);
            StartTestCommand = ReactiveCommand.Create(OpenStartTestWindow);
            OpenResultsTabCommand = ReactiveCommand.Create(OpenResultsTab);

            (CreateTestCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (EditTestCommand as ReactiveCommand<Test, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (DeleteTestCommand as ReactiveCommand<Test, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (TakeTestCommand as ReactiveCommand<Test, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (ViewResultsCommand as ReactiveCommand<TestResult, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (OpenTestListCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (StartTestCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (OpenResultsTabCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);

            LoadData();
        }

        private void HandleCommandException(Exception ex)
        {
            Console.WriteLine($"ошибка команды: {ex.Message}");
        }


        private void LoadData()
        {
            System.Diagnostics.Debug.WriteLine("MainWindowViewModel.LoadData: Загружаем данные");
            
            Tests.Clear();
            var tests = _testService.GetTests();
            
            foreach (var test in tests)
            {
                Tests.Add(test);
            }

            Results.Clear();
            var results = _resultService.GetResults();
            System.Diagnostics.Debug.WriteLine($"MainWindowViewModel.LoadData: Загружено {results.Count} результатов из сервиса");
            
            foreach (var result in results)
            {
                Results.Add(result);
                System.Diagnostics.Debug.WriteLine($"  - Результат: {result.UserName}, {result.Score}/{result.MaxScore}, {result.CompletionDate}");
            }
            
            System.Diagnostics.Debug.WriteLine($"MainWindowViewModel.LoadData: Общее количество результатов в коллекции: {Results.Count}");
        }

        private static Window GetMainWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
                   ?? throw new InvalidOperationException("Main window not found");
        }

        private void CreateTest()
        {
            var editorViewModel = new TestEditorViewModel(_testService);
            var editorWindow = new TestEditorWindow(editorViewModel);
            editorWindow.ShowDialog(GetMainWindow());
            LoadData();
        }

        private void EditTest(Test test)
        {
            var editorViewModel = new TestEditorViewModel(_testService, test);
            var editorWindow = new TestEditorWindow(editorViewModel);

            editorWindow.ShowDialog(GetMainWindow());
            LoadData();
        }

        private void DeleteTest(Test test)
        {
            _testService.DeleteTest(test.Id);
            LoadData();
        }

        private void TakeTest(Test test)
        {
            var vmFactory = ServiceProvider.Instance.GetRequiredService<Func<Test, TestRunnerViewModel>>();
            var runnerViewModel = vmFactory(test);
            var runnerWindow = new TestRunnerWindow(runnerViewModel);

            runnerWindow.ShowDialog(GetMainWindow());
            LoadData();
        }

        private void ViewResult(TestResult result)
        {
            var test = _testService.GetTests().FirstOrDefault(t => t.Id == result.TestId);
            var resultViewModel = new ResultViewModel(result, test);
            var resultWindow = new ResultWindow(resultViewModel);

            resultWindow.ShowDialog(GetMainWindow());
        }

        private void OpenStartTestWindow()
        {
            var testListWindow = new TestListWindow(_testService, _resultService, true);
            testListWindow.ShowDialog(GetMainWindow());
            LoadData();
        }

        private void OpenTestList()
        {
            var testListWindow = new TestListWindow(_testService);
            testListWindow.ShowDialog(GetMainWindow());
            LoadData();
        }

        private void OpenResultsTab()
        {
            System.Diagnostics.Debug.WriteLine("MainWindowViewModel.OpenResultsTab: Открываем окно результатов");
            
            try
            {
                var resultsListViewModel = new ResultsListViewModel(_resultService, _testService, GetMainWindow());
                var resultsListWindow = new ResultsListWindow(resultsListViewModel);
                resultsListWindow.ShowDialog(GetMainWindow());
                
                System.Diagnostics.Debug.WriteLine("MainWindowViewModel.OpenResultsTab: Окно результатов успешно открыто");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindowViewModel.OpenResultsTab: Ошибка открытия окна результатов: {ex}");
                Console.WriteLine($"Ошибка открытия окна результатов: {ex.Message}");
            }
        }
    }
}