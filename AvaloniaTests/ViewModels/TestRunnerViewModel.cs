using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaTests.ViewModels
{
    public class TestRunnerViewModel : ReactiveObject
    {
        private readonly Test _test;
        private readonly IResultService _resultService;
        private readonly IDialogService _dialogService;
        private int _currentQuestionIndex;
        private Dictionary<Guid, Guid> _userAnswers = new();
        private Guid? _selectedAnswer;

        //Текущий вопрос
        public Question CurrentQuestion => _test.Questions[_currentQuestionIndex];
        public string TestTitle => _test.Title;
        public int QuestionNumber => _currentQuestionIndex + 1;
        public int TotalQuestions => _test.Questions.Count;

        // ID выбранного ответа
        public Guid? SelectedAnswer
        {
            get => _selectedAnswer;
            set => this.RaiseAndSetIfChanged(ref _selectedAnswer, value);
        }

        public ICommand NextQuestionCommand { get; private set; }
        public ICommand PreviousQuestionCommand { get; private set; }
        public ICommand FinishTestCommand { get; private set; }
        public ICommand SelectAnswerCommand { get; private set; }

        public TestRunnerViewModel(Test test, IResultService resultService)
        {
            _test = test;
            _resultService = resultService;
            _dialogService = ServiceProvider.Instance.GetRequiredService<IDialogService>();
            _currentQuestionIndex = 0;

            _test.FixCollections();

            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            NextQuestionCommand = ReactiveCommand.Create(NextQuestion);
            PreviousQuestionCommand = ReactiveCommand.Create(PreviousQuestion);
            FinishTestCommand = ReactiveCommand.CreateFromTask(FinishTestAsync);
            SelectAnswerCommand = ReactiveCommand.Create<Guid>(SelectAnswer);
        }

        private void SelectAnswer(Guid answerId)
        {
            _userAnswers[CurrentQuestion.Id] = answerId;
            SelectedAnswer = answerId;
        }

        private void NextQuestion()
        {
            _currentQuestionIndex++;
            this.RaisePropertyChanged(nameof(CurrentQuestion));
            this.RaisePropertyChanged(nameof(QuestionNumber));
            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;
        }

        private void PreviousQuestion()
        {
            _currentQuestionIndex--;
            this.RaisePropertyChanged(nameof(CurrentQuestion));
            this.RaisePropertyChanged(nameof(QuestionNumber));
            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;
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
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var currentWindow = desktop.Windows.FirstOrDefault(w => w.DataContext == this);
                
                if (currentWindow != null && currentWindow != desktop.MainWindow)
                {
                    currentWindow.Close(true);
                }
            }
        }
    }
}