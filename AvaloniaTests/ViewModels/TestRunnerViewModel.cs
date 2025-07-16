using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestRunnerViewModel : ReactiveObject
    {
        private readonly Test _test;
        private readonly IResultService _resultService;
        private int _currentQuestionIndex;
        private Dictionary<Guid, Guid> _userAnswers = new();
        private Guid? _selectedAnswer;

        public Question CurrentQuestion 
        { 
            get 
            {
                if (_test?.Questions == null || _currentQuestionIndex < 0 || _currentQuestionIndex >= _test.Questions.Count)
                {
                    throw new InvalidOperationException($"Invalid question index: {_currentQuestionIndex}");
                }
                return _test.Questions[_currentQuestionIndex];
            }
        }
        public string TestTitle => _test?.Title ?? "Unknown Test";
        public int QuestionNumber => _currentQuestionIndex + 1;
        public int TotalQuestions => _test?.Questions?.Count ?? 0;

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
            _test = test ?? throw new ArgumentNullException(nameof(test));
            _resultService = resultService ?? throw new ArgumentNullException(nameof(resultService));
            _currentQuestionIndex = 0;

            if (_test.Questions == null || _test.Questions.Count == 0)
            {
                throw new InvalidOperationException("Test must have at least one question");
            }

            _test.FixCollections();

            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;

            NextQuestionCommand = ReactiveCommand.Create(NextQuestion);
            PreviousQuestionCommand = ReactiveCommand.Create(PreviousQuestion);
            FinishTestCommand = ReactiveCommand.Create(FinishTest);
            SelectAnswerCommand = ReactiveCommand.Create<Guid>(SelectAnswer);

            (NextQuestionCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (PreviousQuestionCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (FinishTestCommand as ReactiveCommand<Unit, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
            (SelectAnswerCommand as ReactiveCommand<Guid, Unit>)?.ThrownExceptions.Subscribe(HandleCommandException);
        }

        private void HandleCommandException(Exception ex)
        {
            Console.WriteLine($"Ошибка команды в TestRunnerViewModel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"TestRunnerViewModel.HandleCommandException: {ex}");
        }

        private void SelectAnswer(Guid answerId)
        {
            if (CurrentQuestion != null)
            {
                _userAnswers[CurrentQuestion.Id] = answerId;
                SelectedAnswer = answerId;
            }
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
            System.Diagnostics.Debug.WriteLine("TestRunnerViewModel.FinishTest: Начинаем завершение теста");
            
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

            System.Diagnostics.Debug.WriteLine($"TestRunnerViewModel.FinishTest: Результат создан - {result.Score}/{result.MaxScore}");
            System.Diagnostics.Debug.WriteLine($"TestRunnerViewModel.FinishTest: Пользователь: {result.UserName}");
            System.Diagnostics.Debug.WriteLine($"TestRunnerViewModel.FinishTest: Количество ответов: {result.UserAnswers.Count}");

            try
            {
                _resultService.SaveResult(result);
                System.Diagnostics.Debug.WriteLine("TestRunnerViewModel.FinishTest: Результат успешно сохранен");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestRunnerViewModel.FinishTest: Ошибка сохранения результата: {ex}");
                Console.WriteLine($"Ошибка сохранения результата: {ex.Message}");
            }

            var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
            var dialog = new Window
            {
                Title = "Test Completed",
                Content = $"You scored {result.Score} out of {result.MaxScore}!",
                SizeToContent = SizeToContent.WidthAndHeight
            };
            dialog.ShowDialog(mainWindow);

            CloseTestWindow();
        }

        private void CloseTestWindow()
        {
            var windows = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
            var currentWindow = windows?.FirstOrDefault(w => w.DataContext == this);
            if (currentWindow != null)
            {
                System.Diagnostics.Debug.WriteLine("TestRunnerViewModel.CloseTestWindow: Закрываем окно теста");
                currentWindow.Close();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("TestRunnerViewModel.CloseTestWindow: Окно теста не найдено");
            }
        }
    }
}