using Avalonia.Controls;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestListViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService? _resultService;
        private readonly Window _window;
        private readonly bool _selectMode;
        
        private ObservableCollection<Test> _tests = new();
        public ObservableCollection<Test> Tests 
        { 
            get => _tests;
            set => this.RaiseAndSetIfChanged(ref _tests, value);
        }

        public ICommand EditTestCommand { get; }
        public ICommand DeleteTestCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand CreateTestCommand { get; }
        public ICommand SelectTestCommand { get; }
        public ICommand RefreshCommand { get; }

        public TestListViewModel(ITestService testService, Window window, bool selectMode = false, IResultService? resultService = null)
        {
            _testService = testService;
            _window = window;
            _selectMode = selectMode;
            _resultService = resultService;
            
            EditTestCommand = ReactiveCommand.Create<Test>(EditTest);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            CloseCommand = ReactiveCommand.Create(() => _window.Close());
            CreateTestCommand = ReactiveCommand.Create(CreateTest);
            SelectTestCommand = ReactiveCommand.Create<Test>(SelectTest);
            RefreshCommand = ReactiveCommand.Create(Refresh);

            LoadTests();
        }

        public void LoadTests()
        {
            var testsFromService = _testService.GetTests();

            Tests.Clear();

            foreach (var test in testsFromService)
            {
                if (test.Questions == null)
                {
                    test.FixCollections();
                }
                
                Tests.Add(test);
            }
            
            this.RaisePropertyChanged(nameof(Tests));
        }

        public void Refresh()
        {
            LoadTests();
        }

        private void CreateTest()
        {
            var vm = new TestEditorViewModel(_testService);
            var win = new TestEditorWindow(vm);
            win.ShowDialog(_window);
            LoadTests();
        }

        private void EditTest(Test test)
        {
            var vm = new TestEditorViewModel(_testService, test);
            var win = new TestEditorWindow(vm);
            win.ShowDialog(_window);
            LoadTests();
        }

        private void DeleteTest(Test test)
        {
            _testService.DeleteTest(test.Id);
            LoadTests();
        }

        private void SelectTest(Test test)
        {
            if (_selectMode && _resultService != null)
            {
                if (test.Questions == null || test.Questions.Count == 0)
                {
                    return;
                }

                foreach (var question in test.Questions)
                {
                    if (question.Answers == null || question.Answers.Count == 0)
                    {
                        return;
                    }
                }

                var vm = new TestRunnerViewModel(test, _resultService);
                var win = new TestRunnerWindow(vm);
                
                var mainWindow = (Avalonia.Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                if (mainWindow != null)
                {
                    win.WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner;
                    win.ShowDialog(mainWindow);
                }
                else
                {
                    win.Show();
                }
                
                _window.Close();
            }
        }
    }
}
