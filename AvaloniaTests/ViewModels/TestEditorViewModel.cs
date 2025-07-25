using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestEditorViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IDialogService _dialogService;
        private Test _editingTest = null!;

        public Test EditingTest
        {
            get => _editingTest;
            set => this.RaiseAndSetIfChanged(ref _editingTest, value);
        }

        // Свойство для валидации
        public bool CanSaveTest => !string.IsNullOrWhiteSpace(EditingTest?.Title) && 
                                   EditingTest.Questions.Count > 0;

        public ICommand SaveCommand { get; private set; }
        public ICommand AddQuestionCommand { get; private set; }
        public ICommand RemoveQuestionCommand { get; private set; }
        public ICommand AddAnswerCommand { get; private set; }
        public ICommand RemoveAnswerCommand { get; private set; }
        public ICommand EditQuestionCommand { get; private set; }
        public ICommand SetCorrectAnswerCommand { get; private set; }

        public event EventHandler<bool>? CloseRequested;

        public TestEditorViewModel(ITestService testService, IDialogService dialogService, Test? testToEdit = null)
        {
            _testService = testService;
            _dialogService = dialogService;
            EditingTest = testToEdit ?? new Test("", "");

            InitializeCommands();
            SetupValidation();
        }

        private void SetupValidation()
        {
            this.WhenAnyValue(x => x.EditingTest.Title, x => x.EditingTest.Questions.Count)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(CanSaveTest)));

            EditingTest.Questions.CollectionChanged += (s, e) =>
            {
                this.RaisePropertyChanged(nameof(CanSaveTest));
            };
        }

        private void InitializeCommands()
        {
            SaveCommand = ReactiveCommand.Create(SafeSaveTest, this.WhenAnyValue(x => x.CanSaveTest));
            
            AddQuestionCommand = ReactiveCommand.CreateFromTask(AddQuestionAsync);
            RemoveQuestionCommand = ReactiveCommand.Create<Question>(RemoveQuestion);
            AddAnswerCommand = ReactiveCommand.Create<Question>(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer);
            EditQuestionCommand = ReactiveCommand.CreateFromTask<Question>(EditQuestionAsync);
            SetCorrectAnswerCommand = ReactiveCommand.Create<object[]>(SetCorrectAnswer);
        }
        
        private void SafeSaveTest()
        {
            if (!CanSaveTest)
            {
                return;
            }

            EditingTest.QuestionsData = EditingTest.Questions.ToList();
            
            foreach (var question in EditingTest.Questions)
            {
                if (question.Id == Guid.Empty)
                    question.Id = Guid.NewGuid();

                question.AnswersData = question.Answers.ToList();
                
                foreach (var answer in question.Answers)
                {
                    if (answer.Id == Guid.Empty)
                        answer.Id = Guid.NewGuid();
                }
            }

            _testService.SaveTest(EditingTest);
            RequestClose(true);
        }

        private void RequestClose(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }

        private async Task AddQuestionAsync()
        {
            var question = await _dialogService.ShowQuestionEditorAsync();
            if (question != null)
            {
                EditingTest.Questions.Add(question);
                UpdateValidationProperties();
            }
        }

        private void RemoveQuestion(Question question)
        {
            EditingTest.Questions.Remove(question);
            UpdateValidationProperties();
        }

        private void AddAnswer(Question question)
        {
            question.Answers.Add(new Answer(""));
            UpdateValidationProperties();
        }

        private void RemoveAnswer(Answer answer)
        {
            foreach (var question in EditingTest.Questions)
            {
                if (question.Answers.Contains(answer))
                {
                    question.Answers.Remove(answer);
                    if (question.CorrectAnswerId == answer.Id)
                    {
                        question.CorrectAnswerId = question.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
                    }
                    
                    UpdateValidationProperties();
                    break;
                }
            }
        }

        private async Task EditQuestionAsync(Question question)
        {
            var editedQuestion = await _dialogService.ShowQuestionEditorAsync(question);
            if (editedQuestion != null)
            {
                question.Text = editedQuestion.Text;
                question.Answers.Clear();
                foreach (var answer in editedQuestion.Answers)
                {
                    question.Answers.Add(answer);
                }
                question.CorrectAnswerId = editedQuestion.CorrectAnswerId;
                
                UpdateValidationProperties();
            }
        }

        private void SetCorrectAnswer(object[] parameters)
        {
            if (parameters.Length == 2 && parameters[0] is Question question && parameters[1] is Answer answer)
            {
                question.CorrectAnswerId = answer.Id;
                UpdateValidationProperties();
            }
        }

        private void UpdateValidationProperties()
        {
            this.RaisePropertyChanged(nameof(EditingTest));
            this.RaisePropertyChanged(nameof(EditingTest.Questions));
            this.RaisePropertyChanged(nameof(CanSaveTest));
        }
    }
}