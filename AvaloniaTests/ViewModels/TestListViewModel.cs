using Avalonia.Controls;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;

namespace AvaloniaTests.ViewModels
{
    public class TestListViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService? _resultService;
        private readonly IWindowService _windowService;
        private readonly Window _window;
        private readonly bool _selectMode;
        
        private ObservableCollection<Test> _tests = new();
        public ObservableCollection<Test> Tests 
        { 
            get => _tests;
            set => this.RaiseAndSetIfChanged(ref _tests, value);
        }

        public bool IsSelectMode => _selectMode;

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
            _windowService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IWindowService>(ServiceProvider.Instance);
            
            EditTestCommand = ReactiveCommand.CreateFromTask<Test>(EditTestAsync);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            CloseCommand = ReactiveCommand.Create(() => _window.Close());
            CreateTestCommand = ReactiveCommand.CreateFromTask(CreateTestAsync);
            SelectTestCommand = ReactiveCommand.CreateFromTask<Test>(SelectTestAsync);
            RefreshCommand = ReactiveCommand.Create(LoadTests);

            LoadTests();
        }

        public void LoadTests()
        {
            var testsFromService = _testService.GetTests();
            Tests.Clear();

            foreach (var test in testsFromService)
            {
                test.FixCollections();
                Tests.Add(test);
            }
            
            this.RaisePropertyChanged(nameof(Tests));
        }

        private async System.Threading.Tasks.Task CreateTestAsync()
        {
            var result = await _windowService.ShowTestEditorAsync();
            if (result)
            {
                LoadTests();
            }
        }

        private async System.Threading.Tasks.Task EditTestAsync(Test test)
        {
            var result = await _windowService.ShowTestEditorAsync(test);
            if (result)
            {
                LoadTests();
            }
        }

        private void DeleteTest(Test test)
        {
            _testService.DeleteTest(test.Id);
            LoadTests();
        }

        private async System.Threading.Tasks.Task SelectTestAsync(Test test)
        {
            if (_selectMode && _resultService != null)
            {
                var result = await _windowService.ShowTestRunnerAsync(test);
                if (result)
                {
                    _window.Close();
                }
            }
        }
    }
}
