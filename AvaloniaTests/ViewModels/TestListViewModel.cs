using Avalonia.Controls;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System;
using System.Reactive;

namespace AvaloniaTests.ViewModels
{
    public class TestListViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IResultService? _resultService;
        private readonly IWindowService _windowService;
        private readonly IErrorDialogService _errorDialogService;
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
            _errorDialogService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IErrorDialogService>(ServiceProvider.Instance);
            
            EditTestCommand = ReactiveCommand.CreateFromTask<Test>(EditTestAsync);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            CloseCommand = ReactiveCommand.Create(() => _window.Close());
            CreateTestCommand = ReactiveCommand.CreateFromTask(CreateTestAsync);
            SelectTestCommand = ReactiveCommand.CreateFromTask<Test>(SelectTestAsync);
            RefreshCommand = ReactiveCommand.Create(Refresh);

            SubscribeToCommandErrors(EditTestCommand);
            SubscribeToCommandErrors(DeleteTestCommand);
            SubscribeToCommandErrors(CloseCommand);
            SubscribeToCommandErrors(CreateTestCommand);
            SubscribeToCommandErrors(SelectTestCommand);
            SubscribeToCommandErrors(RefreshCommand);

            LoadTests();
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
            System.Diagnostics.Debug.WriteLine($"Ошибка в команде TestList: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

            try
            {
                _errorDialogService.ShowError("Ошибка", $"Произошла ошибка: {ex.Message}");
            }
            catch
            {
            }
        }

        public void LoadTests()
        {
            try
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
            catch (Exception ex)
            {
                HandleCommandException(ex);
            }
        }

        public void Refresh()
        {
            try
            {
                LoadTests();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async System.Threading.Tasks.Task CreateTestAsync()
        {
            try
            {
                var result = await _windowService.ShowTestEditorAsync();
                if (result)
                {
                    LoadTests();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async System.Threading.Tasks.Task EditTestAsync(Test test)
        {
            try
            {
                var result = await _windowService.ShowTestEditorAsync(test);
                if (result)
                {
                    LoadTests();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void DeleteTest(Test test)
        {
            try
            {
                _testService.DeleteTest(test.Id);
                LoadTests();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async System.Threading.Tasks.Task SelectTestAsync(Test test)
        {
            try
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

                    var result = await _windowService.ShowTestRunnerAsync(test);
                    if (result)
                    {
                        _window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
