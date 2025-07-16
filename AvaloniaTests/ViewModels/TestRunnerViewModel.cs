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
            _currentQuestionIndex = 0;

            _test.FixCollections();

            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;

            NextQuestionCommand = ReactiveCommand.Create(NextQuestion);
            PreviousQuestionCommand = ReactiveCommand.Create(PreviousQuestion);
            FinishTestCommand = ReactiveCommand.Create(FinishTest);
            SelectAnswerCommand = ReactiveCommand.Create<Guid>(SelectAnswer);
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
            currentWindow?.Close();
        }
    }
}