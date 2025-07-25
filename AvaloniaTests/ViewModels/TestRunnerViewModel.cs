using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestRunnerViewModel : ReactiveObject
    {
        private readonly Test _test;
        private readonly IResultService _resultService;
        private readonly IDialogService _dialogService;
        private readonly string _currentUserName;
        private int _currentQuestionIndex;
        private Dictionary<Guid, Guid> _userAnswers = new();
        private Guid? _selectedAnswer;

        //“екущий вопрос
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

        // —войства дл€ управлени€ видимостью и доступностью кнопок навигации
        public bool CanGoNext => _currentQuestionIndex < _test.Questions.Count - 1;
        public bool CanGoPrevious => _currentQuestionIndex > 0;
        public bool HasMultipleQuestions => _test.Questions.Count > 1;

        public ICommand NextQuestionCommand { get; private set; }
        public ICommand PreviousQuestionCommand { get; private set; }
        public ICommand FinishTestCommand { get; private set; }
        public ICommand SelectAnswerCommand { get; private set; }

        public event EventHandler<bool>? CloseRequested;

        public TestRunnerViewModel(Test test, IResultService resultService, IDialogService dialogService, string currentUserName = "ѕользователь")
        {
            _test = test;
            _resultService = resultService;
            _dialogService = dialogService;
            _currentUserName = currentUserName;
            _currentQuestionIndex = 0;

            _test.FixCollections();

            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            NextQuestionCommand = ReactiveCommand.Create(NextQuestion, this.WhenAnyValue(x => x.CanGoNext));
            PreviousQuestionCommand = ReactiveCommand.Create(PreviousQuestion, this.WhenAnyValue(x => x.CanGoPrevious));
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
            if (_currentQuestionIndex < _test.Questions.Count - 1)
            {
                _currentQuestionIndex++;
                UpdateCurrentQuestionProperties();
            }
        }

        private void PreviousQuestion()
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                UpdateCurrentQuestionProperties();
            }
        }

        private void UpdateCurrentQuestionProperties()
        {
            this.RaisePropertyChanged(nameof(CurrentQuestion));
            this.RaisePropertyChanged(nameof(QuestionNumber));
            this.RaisePropertyChanged(nameof(CanGoNext));
            this.RaisePropertyChanged(nameof(CanGoPrevious));
            
            SelectedAnswer = _userAnswers.TryGetValue(CurrentQuestion.Id, out var answer) ? answer : null;
        }

        private async System.Threading.Tasks.Task FinishTestAsync()
        {
            var result = new TestResult
            {
                TestId = _test.Id,
                UserName = _currentUserName,
                CompletionDate = DateTime.Now,
                MaxScore = _test.Questions.Count,
                UserAnswers = _userAnswers
            };

            result.Score = _test.Questions.Count(q =>
                _userAnswers.TryGetValue(q.Id, out var answerId) && answerId == q.CorrectAnswerId);

            _resultService.SaveResult(result);

            await _dialogService.ShowTestCompletionDialogAsync(result);

            RequestClose(true);
        }

        private void RequestClose(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }
    }
}