using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.Views;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Controls;
using System;
using System.Reactive;

namespace AvaloniaTests.ViewModels
{
    public class ResultsListViewModel : ViewModelBase
    {
        private readonly IResultService _resultService;
        private readonly ITestService _testService;
        private readonly Window? _parentWindow;
        private Window? _currentWindow;
        private int _resultsCount;

        // Коллекция отображаемых результатов
        public ObservableCollection<TestResultDisplayItem> Results { get; } = new();

        public int ResultsCount
        {
            get => _resultsCount;
            set => this.RaiseAndSetIfChanged(ref _resultsCount, value);
        }

        public bool HasNoResults => Results.Count == 0;

        public ICommand ViewResultCommand { get; }
        public ICommand DeleteResultCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand RefreshCommand { get; }

        public ResultsListViewModel(IResultService resultService, ITestService testService, Window? parentWindow = null)
        {
            _resultService = resultService;
            _testService = testService;
            _parentWindow = parentWindow;

            ViewResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(ViewResult);
            DeleteResultCommand = ReactiveCommand.Create<TestResultDisplayItem>(DeleteResult);
            CloseCommand = ReactiveCommand.Create(CloseWindow);
            RefreshCommand = ReactiveCommand.Create(LoadResults);

            SubscribeToCommandErrors(ViewResultCommand);
            SubscribeToCommandErrors(DeleteResultCommand);
            SubscribeToCommandErrors(CloseCommand);
            SubscribeToCommandErrors(RefreshCommand);

            Results.CollectionChanged += (s, e) => 
            {
                this.RaisePropertyChanged(nameof(HasNoResults));
            };

            LoadResults();
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
            System.Diagnostics.Debug.WriteLine($"Ошибка в команде ResultsList: {ex.Message}");
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
                errorWindow.ShowDialog(_currentWindow ?? _parentWindow);
            }
            catch
            {
            }
        }

        public void SetCurrentWindow(Window window)
        {
            _currentWindow = window;
        }

        private void CloseWindow()
        {
            try
            {
                _currentWindow?.Close();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void DeleteResult(TestResultDisplayItem item)
        {
            try
            {
                if (item?.Result != null)
                {
                    _resultService.DeleteResult(item.Result.Id);
                    Results.Remove(item);
                    ResultsCount = Results.Count;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void LoadResults()
        {
            try
            {
                Results.Clear();
                
                var results = _resultService.GetResults();
                var tests = _testService.GetTests();

                foreach (var result in results.OrderByDescending(r => r.CompletionDate))
                {
                    var test = tests.FirstOrDefault(t => t.Id == result.TestId);
                    var displayItem = new TestResultDisplayItem
                    {
                        Result = result,
                        Test = test,
                        TestTitle = test?.Title ?? "Неизвестный тест",
                        UserName = result.UserName,
                        Score = $"{result.Score}/{result.MaxScore}",
                        CompletionDate = result.CompletionDate.ToString("dd.MM.yyyy HH:mm"),
                        Percentage = result.MaxScore > 0 
                            ? (int)((double)result.Score / result.MaxScore * 100) 
                            : 0
                    };
                    
                    Results.Add(displayItem);
                }

                ResultsCount = Results.Count;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ViewResult(TestResultDisplayItem item)
        {
            try
            {
                if (item?.Result != null)
                {
                    var resultViewModel = new ResultViewModel(item.Result, item.Test);
                    var resultWindow = new ResultWindow(resultViewModel);
                    resultWindow.ShowDialog(_currentWindow ?? _parentWindow);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    // Отображение информации о результате тестирования в списке
    public class TestResultDisplayItem : ViewModelBase
    {
        private TestResult? _result;
        private Test? _test;
        private string _testTitle = "";
        private string _userName = "";
        private string _score = "";
        private string _completionDate = "";
        private int _percentage;

        public TestResult? Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        public Test? Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }

        public string TestTitle
        {
            get => _testTitle;
            set => this.RaiseAndSetIfChanged(ref _testTitle, value);
        }

        public string UserName
        {
            get => _userName;
            set => this.RaiseAndSetIfChanged(ref _userName, value);
        }

        public string Score
        {
            get => _score;
            set => this.RaiseAndSetIfChanged(ref _score, value);
        }

        public string CompletionDate
        {
            get => _completionDate;
            set => this.RaiseAndSetIfChanged(ref _completionDate, value);
        }

        public int Percentage
        {
            get => _percentage;
            set => this.RaiseAndSetIfChanged(ref _percentage, value);
        }
    }
}