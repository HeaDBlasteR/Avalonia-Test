using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Reactive;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaTests.ViewModels
{
    public class TestRunnerViewModel : ReactiveObject
    {
        private readonly Test _test;
        private readonly IResultService _resultService;
        private readonly IErrorDialogService _errorDialogService;
        private int _currentQuestionIndex;
        private Dictionary<Guid, Guid> _userAnswers = new();
        private Guid? _selectedAnswer;

        public Question CurrentQuestion 
        { 
            get 
            {
                return _test.Questions[_currentQuestionIndex];
            }
        }
        public string TestTitle => _test.Title;
        public int QuestionNumber => _currentQuestionIndex + 1;
        public int TotalQuestions => _test.Questions.Count;

        public Guid? SelectedAnswer
        {
            get => _selectedAnswer;
            set => this.RaiseAndSetIfChanged(ref _selectedAnswer, value);
        }

        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }
        public ICommand FinishTestCommand { get; }
        public ICommand SelectAnswerCommand { get; }

        public TestRunnerViewModel(Test test, IResultService resultService)
        {
            _test = test;
            _resultService = resultService;
            _errorDialogService = ServiceProvider.Instance.GetRequiredService<IErrorDialogService>();
            _currentQuestionIndex = 0;

            _test.FixCollections();

            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;

            NextQuestionCommand = ReactiveCommand.Create(NextQuestion);
            PreviousQuestionCommand = ReactiveCommand.Create(PreviousQuestion);
            FinishTestCommand = ReactiveCommand.Create(FinishTest);
            SelectAnswerCommand = ReactiveCommand.Create<Guid>(SelectAnswer);

            SubscribeToCommandErrors(NextQuestionCommand);
            SubscribeToCommandErrors(PreviousQuestionCommand);
            SubscribeToCommandErrors(FinishTestCommand);
            SubscribeToCommandErrors(SelectAnswerCommand);
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

        private void SelectAnswer(Guid answerId)
        {
            _userAnswers[CurrentQuestion.Id] = answerId;
            SelectedAnswer = answerId;
        }

        private void NextQuestion()
        {
            if (_currentQuestionIndex < _test.Questions.Count - 1)
            {
                _currentQuestionIndex++;
                this.RaisePropertyChanged(nameof(CurrentQuestion));
                this.RaisePropertyChanged(nameof(QuestionNumber));
                SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;
            }
        }

        private void PreviousQuestion()
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                this.RaisePropertyChanged(nameof(CurrentQuestion));
                this.RaisePropertyChanged(nameof(QuestionNumber));
                SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;
            }
        }

        private void FinishTest()
        {
            var result = new TestResult
            {
                TestId = _test.Id,
                UserName = Environment.UserName,
                CompletionDate = DateTime.Now,
                MaxScore = _test.Questions.Count,
                UserAnswers = _userAnswers
            };

            result.Score = _test.Questions.Count(q =>
                _userAnswers.TryGetValue(q.Id, out var answerId) && answerId == q.CorrectAnswerId);

            _resultService.SaveResult(result);

            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
            
            var dialog = new Window
            {
                Title = "Результат теста",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F5F5DC")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black),
                CanResize = false
            };

            var stackPanel = new StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 15
            };

            var titleText = new TextBlock
            {
                Text = "🎯 Тест завершен!",
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black)
            };

            var scoreText = new TextBlock
            {
                Text = $"Ваш результат: {result.Score} из {result.MaxScore}",
                FontSize = 16,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black)
            };

            var percentageText = new TextBlock
            {
                Text = $"Процент: {(result.MaxScore > 0 ? (int)((double)result.Score / result.MaxScore * 100) : 0)}%",
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF4CAF50")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };

            okButton.Click += (_, __) => dialog.Close();

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(scoreText);
            stackPanel.Children.Add(percentageText);
            stackPanel.Children.Add(okButton);

            dialog.Content = stackPanel;
            dialog.ShowDialog(mainWindow);

            CloseTestWindow();
        }

        private void CloseTestWindow()
        {
            var windows = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
            var currentWindow = windows?.FirstOrDefault(w => w.DataContext == this);
            currentWindow?.Close();
        }
    }
}