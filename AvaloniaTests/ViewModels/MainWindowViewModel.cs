using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.Views;
using AvaloniaTests.Services;
using Microsoft.Extensions.DependencyInjection;
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

        public MainWindowViewModel(ITestService testService, IResultService resultService, IErrorDialogService errorDialogService)
        {
            _testService = testService;
            _resultService = resultService;
            _errorDialogService = errorDialogService;

            CreateTestCommand = ReactiveCommand.Create(CreateTest);
            EditTestCommand = ReactiveCommand.Create<Test>(EditTest);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            TakeTestCommand = ReactiveCommand.Create<Test>(TakeTest);
            ViewResultsCommand = ReactiveCommand.Create<TestResult>(ViewResult);
            OpenTestListCommand = ReactiveCommand.Create(OpenTestList);
            StartTestCommand = ReactiveCommand.Create(OpenStartTestWindow);
            OpenResultsTabCommand = ReactiveCommand.Create(OpenResultsTab);

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

        private static Window GetMainWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;
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
            var resultsListViewModel = new ResultsListViewModel(_resultService, _testService, GetMainWindow());
            var resultsListWindow = new ResultsListWindow(resultsListViewModel);
            resultsListWindow.ShowDialog(GetMainWindow());
        }
    }
}