using Avalonia;
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
        private readonly IDialogService _dialogService;
        private int _currentQuestionIndex;
        private Dictionary<Guid, Guid> _userAnswers = new();
        private Guid? _selectedAnswer;

        //Текущий вопрос
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

        // ID выбранного ответа
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
            _dialogService = ServiceProvider.Instance.GetRequiredService<IDialogService>();
            _currentQuestionIndex = 0;

            _test.FixCollections();

            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;

            NextQuestionCommand = ReactiveCommand.Create(NextQuestion);
            PreviousQuestionCommand = ReactiveCommand.Create(PreviousQuestion);
            FinishTestCommand = ReactiveCommand.CreateFromTask(FinishTestAsync);
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

        private async System.Threading.Tasks.Task FinishTestAsync()
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

            await _dialogService.ShowTestCompletionDialogAsync(result);

            CloseTestWindow();
        }

        private void CloseTestWindow()
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var currentWindow = desktop.Windows.FirstOrDefault(w => w.DataContext == this);
                    
                    if (currentWindow != null && currentWindow != desktop.MainWindow)
                    {
                        currentWindow.Close(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии окна теста: {ex.Message}");
            }
        }
    }
}