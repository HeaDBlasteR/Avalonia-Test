using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestListViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService? _resultService;
        private readonly IWindowService _windowService;
        private readonly bool _selectMode;
        
        private ObservableCollection<Test> _tests = new();
        public ObservableCollection<Test> Tests 
        { 
            get => _tests;
            set => this.RaiseAndSetIfChanged(ref _tests, value);
        }

        public bool IsSelectMode => _selectMode;

        public ICommand EditTestCommand { get; private set; }
        public ICommand DeleteTestCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand CreateTestCommand { get; private set; }
        public ICommand SelectTestCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        // Событие для запроса закрытия окна
        public event EventHandler? CloseRequested;

        public TestListViewModel(ITestService testService, bool selectMode = false, IResultService? resultService = null)
        {
            _testService = testService;
            _selectMode = selectMode;
            _resultService = resultService;
            _windowService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IWindowService>(ServiceProvider.Instance);
            
            InitializeCommands();
            LoadTests();
        }

        private void InitializeCommands()
        {
            EditTestCommand = ReactiveCommand.CreateFromTask<Test>(EditTestAsync);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            CloseCommand = ReactiveCommand.Create(RequestClose);
            CreateTestCommand = ReactiveCommand.CreateFromTask(CreateTestAsync);
            SelectTestCommand = ReactiveCommand.CreateFromTask<Test>(SelectTestAsync);
            RefreshCommand = ReactiveCommand.Create(LoadTests);
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
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
                    RequestClose();
                }
            }
        }
    }
}
