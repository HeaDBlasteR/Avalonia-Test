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

            (EditTestCommand as ReactiveCommand<Test, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (DeleteTestCommand as ReactiveCommand<Test, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (CloseCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (CreateTestCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (SelectTestCommand as ReactiveCommand<Test, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (RefreshCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);

            System.Diagnostics.Debug.WriteLine("TestListViewModel: ����������� ������");
            
            LoadTests();
        }

        private void HandleCommandException(Exception ex)
        {
            Console.WriteLine($"������ �������: {ex.Message}");
        }

        public void LoadTests()
        {
            System.Diagnostics.Debug.WriteLine("TestListViewModel.LoadTests: �������� �������� ������");
            
            var testsFromService = _testService.GetTests();
            
            System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: �������� {testsFromService.Count} ������ �� �������");

            Tests.Clear();

            foreach (var test in testsFromService)
            {
                if (test.Questions == null)
                {
                    System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: ���������� ��������� ��� ����� '{test.Title}'");
                    test.FixCollections();
                }
                
                System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: ��������� ���� '{test.Title}' � {test.Questions?.Count ?? 0} ���������");
                Tests.Add(test);
                
                System.Diagnostics.Debug.WriteLine($"  - ID: {test.Id}");
                System.Diagnostics.Debug.WriteLine($"  - Title: '{test.Title}'");
                System.Diagnostics.Debug.WriteLine($"  - Description: '{test.Description}'");
                System.Diagnostics.Debug.WriteLine($"  - Questions Count: {test.Questions?.Count ?? 0}");
            }
            
            System.Diagnostics.Debug.WriteLine($"TestListViewModel.LoadTests: ���������. ����� ������ � ObservableCollection: {Tests.Count}");
            
            for (int i = 0; i < Tests.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine($"  ���� {i}: '{Tests[i].Title}'");
            }
            
            this.RaisePropertyChanged(nameof(Tests));
        }

        public void Refresh()
        {
            System.Diagnostics.Debug.WriteLine("TestListViewModel.Refresh: ��������� ������ ������");
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
            try
            {
                if (_selectMode && _resultService != null)
                {
                    if (test.Questions == null || test.Questions.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"TestListViewModel.SelectTest: ���� '{test.Title}' �� ����� ��������");
                        return;
                    }

                    foreach (var question in test.Questions)
                    {
                        if (question.Answers == null || question.Answers.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"TestListViewModel.SelectTest: ������ '{question.Text}' �� ����� �������");
                            return;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"TestListViewModel.SelectTest: �������� ���� '{test.Title}'");
                    
                    System.Diagnostics.Debug.WriteLine("TestListViewModel.SelectTest: ������� TestRunnerViewModel");
                    var vm = new TestRunnerViewModel(test, _resultService);
                    
                    System.Diagnostics.Debug.WriteLine("TestListViewModel.SelectTest: ������� TestRunnerWindow");
                    var win = new TestRunnerWindow(vm);
                    
                    var mainWindow = (Avalonia.Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                    if (mainWindow != null)
                    {
                        win.WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner;
                        win.ShowDialog(mainWindow);
                        System.Diagnostics.Debug.WriteLine("TestListViewModel.SelectTest: ���� �������� ��� ������");
                    }
                    else
                    {
                        win.Show();
                        System.Diagnostics.Debug.WriteLine("TestListViewModel.SelectTest: ���� �������� ������� ��������");
                    }
                    
                    System.Diagnostics.Debug.WriteLine("TestListViewModel.SelectTest: ��������� ������������ ����");
                    _window.Close();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"TestListViewModel.SelectTest: ����� ������ �������� ��� ������ ����������� �� ��������. _selectMode={_selectMode}, _resultService={_resultService}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestListViewModel.SelectTest: ������ ��� ������� �����: {ex}");
                Console.WriteLine($"������ ��� ������� �����: {ex.Message}");
                
                try
                {
                    var errorWindow = new Window
                    {
                        Title = "������",
                        Width = 400,
                        Height = 300,
                        Content = new TextBlock 
                        { 
                            Text = $"��������� ������ ��� ������� �����:\n\n{ex.Message}\n\n�����������:\n{ex.StackTrace}",
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(10)
                        }
                    };
                    errorWindow.ShowDialog(_window);
                }
                catch
                {
                }
            }
        }
    }
}
