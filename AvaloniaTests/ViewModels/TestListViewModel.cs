using Avalonia.Controls;
using AvaloniaTests.Models;
using AvaloniaTests.Views;
using AvaloniaTests.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
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
            
            System.Diagnostics.Debug.WriteLine("TestListViewModel: Конструктор вызван");
            
            LoadTests();
        }

        public void LoadTests()
        {
            System.Diagnostics.Debug.WriteLine("TestListViewModel.LoadTests: Начинаем загрузку тестов");
            
            var testsFromService = _testService.GetTests();
            
            System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: Получено {testsFromService.Count} тестов из сервиса");

            Tests.Clear();

            foreach (var test in testsFromService)
            {
                if (test.Questions == null)
                {
                    System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: Исправляем коллекции для теста '{test.Title}'");
                    test.FixCollections();
                }
                
                System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: Добавляем тест '{test.Title}' с {test.Questions?.Count ?? 0} вопросами");
                Tests.Add(test);
                
                System.Diagnostics.Debug.WriteLine($"  - ID: {test.Id}");
                System.Diagnostics.Debug.WriteLine($"  - Title: '{test.Title}'");
                System.Diagnostics.Debug.WriteLine($"  - Description: '{test.Description}'");
                System.Diagnostics.Debug.WriteLine($"  - Questions Count: {test.Questions?.Count ?? 0}");
            }
            
            System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: Завершено. Всего тестов в ObservableCollection: {Tests.Count}");
            
            for (int i = 0; i < Tests.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"  Тест {i}: '{Tests[i].Title}'");
            }
            
            this.RaisePropertyChanged(nameof(Tests));
        }

        public void Refresh()
        {
            System.Diagnostics.Debug.WriteLine("TestListViewModel.Refresh: Обновляем список тестов");
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
                var vm = new TestRunnerViewModel(test, _resultService);
                var win = new TestRunnerWindow(vm);
                win.ShowDialog(_window);
                _window.Close();
            }
        }
    }
}
