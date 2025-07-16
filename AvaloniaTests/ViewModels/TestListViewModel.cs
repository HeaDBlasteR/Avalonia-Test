using Avalonia.Controls;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.Views;
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
            
            EditTestCommand = ReactiveCommand.Create<Test>(EditTest);
            DeleteTestCommand = ReactiveCommand.Create<Test>(DeleteTest);
            CloseCommand = ReactiveCommand.Create(() => _window.Close());
            CreateTestCommand = ReactiveCommand.Create(CreateTest);
            SelectTestCommand = ReactiveCommand.Create<Test>(SelectTest);
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
                ShowErrorDialog("Ошибка", $"Произошла ошибка: {ex.Message}");
            }
            catch
            {
            }
        }

        private void ShowErrorDialog(string title, string message)
        {
            try
            {
                var errorWindow = new Window
                {
                    Title = title,
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false
                };

                errorWindow.Background = new Avalonia.Media.LinearGradientBrush
                {
                    StartPoint = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Relative),
                    EndPoint = new Avalonia.RelativePoint(1, 1, Avalonia.RelativeUnit.Relative),
                    GradientStops = 
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.Parse("#FFE5E5"), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.Parse("#FFCCCC"), 1)
                    }
                };

                var content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(30),
                    Spacing = 20,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var titleText = new TextBlock
                {
                    Text = "?? " + title,
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D32F2F")),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var messageText = new TextBlock
                {
                    Text = message,
                    FontSize = 14,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#424242")),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 100,
                    Height = 35,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D32F2F")),
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    FontWeight = Avalonia.Media.FontWeight.SemiBold,
                    CornerRadius = new Avalonia.CornerRadius(8)
                };

                okButton.Click += (_, __) => errorWindow.Close();

                content.Children.Add(titleText);
                content.Children.Add(messageText);
                content.Children.Add(okButton);

                errorWindow.Content = content;
                errorWindow.ShowDialog(_window);
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

        private void CreateTest()
        {
            try
            {
                var vm = new TestEditorViewModel(_testService);
                var win = new TestEditorWindow(vm);
                win.ShowDialog(_window);
                LoadTests();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void EditTest(Test test)
        {
            try
            {
                var vm = new TestEditorViewModel(_testService, test);
                var win = new TestEditorWindow(vm);
                win.ShowDialog(_window);
                LoadTests();
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

        private void SelectTest(Test test)
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
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
