using AvaloniaTests.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class QuestionEditorViewModel : ViewModelBase
    {
        private Question _editingQuestion = null!;
        private bool _isEditMode;

        public Question EditingQuestion
        {
            get => _editingQuestion;
            set => this.RaiseAndSetIfChanged(ref _editingQuestion, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
        }

        public string WindowTitle => IsEditMode ? "Редактирование вопроса" : "Добавление вопроса";

        public bool CanRemoveAnswers => EditingQuestion?.Answers?.Count > 2;

        // Свойство для валидации
        public bool CanSaveQuestion => !string.IsNullOrWhiteSpace(EditingQuestion?.Text) &&
                                      EditingQuestion.Answers.Count >= 2 &&
                                      EditingQuestion.Answers.All(a => !string.IsNullOrWhiteSpace(a.Text)) &&
                                      EditingQuestion.CorrectAnswerId != Guid.Empty;

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand AddAnswerCommand { get; private set; }
        public ICommand RemoveAnswerCommand { get; private set; }
        public ICommand SetCorrectAnswerCommand { get; private set; }

        public event EventHandler<bool>? CloseRequested;

        public QuestionEditorViewModel(Question? question = null)
        {
            IsEditMode = question != null;
            EditingQuestion = question != null ? CreateCopyOfQuestion(question) : CreateNewQuestion();

            InitializeCommands();
            SetupValidation();
        }

        //Подписываемся на валидацию
        private void SetupValidation()
        {
            this.WhenAnyValue(x => x.EditingQuestion.Text, x => x.EditingQuestion.CorrectAnswerId)
                .Subscribe(_ => UpdateValidationProperties());

            EditingQuestion.Answers.CollectionChanged += (s, e) => UpdateValidationProperties();

            this.WhenAnyValue(x => x.IsEditMode)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(WindowTitle)));
        }

        private void InitializeCommands()
        {
            // SaveCommand зависит от валидации - кнопка автоматически неактивна при нарушении условий
            SaveCommand = ReactiveCommand.Create(Save, this.WhenAnyValue(x => x.CanSaveQuestion));
            
            CancelCommand = ReactiveCommand.Create(Cancel);
            AddAnswerCommand = ReactiveCommand.Create(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer, 
                this.WhenAnyValue(x => x.CanRemoveAnswers));
            SetCorrectAnswerCommand = ReactiveCommand.Create<Answer>(SetCorrectAnswer);
        }

        private Question CreateNewQuestion()
        {
            var question = new Question("");
            question.Answers.Add(new Answer(""));
            question.Answers.Add(new Answer(""));
            question.CorrectAnswerId = Guid.Empty; // Не выбираем правильный ответ по умолчанию
            return question;
        }

        private Question CreateCopyOfQuestion(Question original)
        {
            var copy = new Question(original.Text)
            {
                Id = original.Id,
                CorrectAnswerId = original.CorrectAnswerId
            };

            copy.Answers.Clear();
            foreach (var answer in original.Answers)
            {
                copy.Answers.Add(new Answer(answer.Text) { Id = answer.Id });
            }

            return copy;
        }

        private void Save()
        {
            //Доп проверка
            if (!CanSaveQuestion)
            {
                return;
            }

            if (EditingQuestion.Id == Guid.Empty)
                EditingQuestion.Id = Guid.NewGuid();

            foreach (var answer in EditingQuestion.Answers)
            {
                if (answer.Id == Guid.Empty)
                    answer.Id = Guid.NewGuid();
            }

            EditingQuestion.AnswersData = EditingQuestion.Answers.ToList();

            RequestClose(true);
        }

        private void Cancel()
        {
            RequestClose(false);
        }

        private void RequestClose(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }

        private void AddAnswer()
        {
            EditingQuestion.Answers.Add(new Answer(""));
            UpdateValidationProperties();
        }

        private void RemoveAnswer(Answer answer)
        {
            EditingQuestion.Answers.Remove(answer);
            
            if (EditingQuestion.CorrectAnswerId == answer.Id)
            {
                EditingQuestion.CorrectAnswerId = Guid.Empty;
            }
            
            UpdateValidationProperties();
        }

        private void SetCorrectAnswer(Answer answer)
        {
            EditingQuestion.CorrectAnswerId = answer.Id;
            UpdateValidationProperties();
        }

        private void UpdateValidationProperties()
        {
            this.RaisePropertyChanged(nameof(CanRemoveAnswers));
            this.RaisePropertyChanged(nameof(CanSaveQuestion));
        }
    }
}