using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Linq;
using System.Windows.Input;
using System.Reactive;

namespace AvaloniaTests.ViewModels
{
    public class QuestionEditorViewModel : ViewModelBase
    {
        private readonly IErrorDialogService _errorDialogService;
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddAnswerCommand { get; }
        public ICommand RemoveAnswerCommand { get; }
        public ICommand SetCorrectAnswerCommand { get; }

        public QuestionEditorViewModel(Question? question = null)
        {
            _errorDialogService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                .GetRequiredService<IErrorDialogService>(ServiceProvider.Instance);

            IsEditMode = question != null;
            EditingQuestion = question != null ? CreateCopyOfQuestion(question) : CreateNewQuestion();

            SaveCommand = ReactiveCommand.Create(Save);
            CancelCommand = ReactiveCommand.Create(Cancel);
            AddAnswerCommand = ReactiveCommand.Create(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer);
            SetCorrectAnswerCommand = ReactiveCommand.Create<Answer>(SetCorrectAnswer);

            SubscribeToCommandErrors(SaveCommand);
            SubscribeToCommandErrors(CancelCommand);
            SubscribeToCommandErrors(AddAnswerCommand);
            SubscribeToCommandErrors(RemoveAnswerCommand);
            SubscribeToCommandErrors(SetCorrectAnswerCommand);

            // Подписываемся на изменения коллекции ответов для обновления CanRemoveAnswers
            EditingQuestion.Answers.CollectionChanged += (s, e) => 
            {
                this.RaisePropertyChanged(nameof(CanRemoveAnswers));
            };

            // Подписываемся на изменения IsEditMode для обновления WindowTitle
            this.WhenAnyValue(x => x.IsEditMode)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(WindowTitle)));
        }

        private Question CreateNewQuestion()
        {
            var question = new Question("");
            question.Answers.Add(new Answer(""));
            question.Answers.Add(new Answer(""));
            question.CorrectAnswerId = question.Answers[0].Id;
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

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(EditingQuestion.Text))
            {
                _errorDialogService.ShowError("Ошибка валидации", "Пожалуйста, введите текст вопроса!");
                return;
            }

            if (EditingQuestion.Answers.Count < 2)
            {
                _errorDialogService.ShowError("Ошибка валидации", "Вопрос должен содержать минимум 2 варианта ответа!");
                return;
            }

            if (EditingQuestion.Answers.Any(a => string.IsNullOrWhiteSpace(a.Text)))
            {
                _errorDialogService.ShowError("Ошибка валидации", "Все варианты ответов должны быть заполнены!");
                return;
            }

            if (EditingQuestion.CorrectAnswerId == Guid.Empty || 
                !EditingQuestion.Answers.Any(a => a.Id == EditingQuestion.CorrectAnswerId))
            {
                _errorDialogService.ShowError("Ошибка валидации", "Пожалуйста, выберите правильный ответ!");
                return;
            }

            // Генерируем ID для новых элементов
            if (EditingQuestion.Id == Guid.Empty)
                EditingQuestion.Id = Guid.NewGuid();

            foreach (var answer in EditingQuestion.Answers)
            {
                if (answer.Id == Guid.Empty)
                    answer.Id = Guid.NewGuid();
            }

            // Синхронизируем данные для сериализации
            EditingQuestion.AnswersData = EditingQuestion.Answers.ToList();

            CloseWithResult(true);
        }

        private void Cancel()
        {
            CloseWithResult(false);
        }

        private void AddAnswer()
        {
            EditingQuestion.Answers.Add(new Answer(""));
        }

        private void RemoveAnswer(Answer answer)
        {
            if (EditingQuestion.Answers.Count <= 2)
            {
                _errorDialogService.ShowError("Предупреждение", "Вопрос должен содержать минимум 2 варианта ответа!");
                return;
            }

            EditingQuestion.Answers.Remove(answer);
            
            if (EditingQuestion.CorrectAnswerId == answer.Id)
            {
                EditingQuestion.CorrectAnswerId = EditingQuestion.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
            }
        }

        private void SetCorrectAnswer(Answer answer)
        {
            EditingQuestion.CorrectAnswerId = answer.Id;
        }

        private void CloseWithResult(bool result)
        {
            try
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is 
                    Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var currentWindow = desktop.Windows.FirstOrDefault(w => w.DataContext == this);
                    
                    if (currentWindow != null && currentWindow != desktop.MainWindow)
                    {
                        currentWindow.Close(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при закрытии окна редактора вопросов: {ex.Message}");
            }
        }
    }
}