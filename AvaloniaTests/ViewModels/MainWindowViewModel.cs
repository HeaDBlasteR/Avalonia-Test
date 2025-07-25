using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService _resultService;
        private readonly IWindowService _windowService;

        public ObservableCollection<Test> Tests { get; } = new();
        public ObservableCollection<TestResult> Results { get; } = new();

        public ICommand CreateTestCommand { get; private set; }
        public ICommand EditTestCommand { get; private set; }
        public ICommand DeleteTestCommand { get; private set; }
        public ICommand TakeTestCommand { get; private set; }
        public ICommand ViewResultsCommand { get; private set; }
        public ICommand OpenTestListCommand { get; private set; }
        public ICommand StartTestCommand { get; private set; }
        public ICommand OpenResultsTabCommand { get; private set; }

        public MainWindowViewModel(ITestService testService, IResultService resultService, 
            IErrorDialogService errorDialogService, IWindowService windowService)
        {
            _testService = testService;
            _resultService = resultService;
            _windowService = windowService;

            InitializeCommands();
            LoadData();
        }

        private void InitializeCommands()
        {
            CreateTestCommand = ReactiveCommand.CreateFromTask(CreateTestAsync);
            EditTestCommand = ReactiveCommand.CreateFromTask<Test>(EditTestAsync);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            TakeTestCommand = ReactiveCommand.CreateFromTask<Test>(TakeTestAsync);
            ViewResultsCommand = ReactiveCommand.CreateFromTask<TestResult>(ViewResultAsync);
            OpenTestListCommand = ReactiveCommand.CreateFromTask(OpenTestListAsync);
            StartTestCommand = ReactiveCommand.CreateFromTask(OpenStartTestWindowAsync);
            OpenResultsTabCommand = ReactiveCommand.CreateFromTask(OpenResultsTabAsync);
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