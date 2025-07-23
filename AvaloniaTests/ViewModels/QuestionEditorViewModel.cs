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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddAnswerCommand { get; }
        public ICommand RemoveAnswerCommand { get; }
        public ICommand SetCorrectAnswerCommand { get; }

        public QuestionEditorViewModel(Question? question = null)
        {
            IsEditMode = question != null;
            EditingQuestion = question != null ? CreateCopyOfQuestion(question) : CreateNewQuestion();

            SaveCommand = ReactiveCommand.Create(Save);
            CancelCommand = ReactiveCommand.Create(Cancel);
            AddAnswerCommand = ReactiveCommand.Create(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer);
            SetCorrectAnswerCommand = ReactiveCommand.Create<Answer>(SetCorrectAnswer);

            EditingQuestion.Answers.CollectionChanged += (s, e) => 
            {
                this.RaisePropertyChanged(nameof(CanRemoveAnswers));
            };

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

        private void Save()
        {
            if (EditingQuestion.Id == Guid.Empty)
                EditingQuestion.Id = Guid.NewGuid();

            foreach (var answer in EditingQuestion.Answers)
            {
                if (answer.Id == Guid.Empty)
                    answer.Id = Guid.NewGuid();
            }

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
            EditingQuestion.Answers.Remove(answer);
            
            if (EditingQuestion.CorrectAnswerId == answer.Id)
            {
                EditingQuestion.CorrectAnswerId = EditingQuestion.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
            }
            
            this.RaisePropertyChanged(nameof(CanRemoveAnswers));
        }

        private void SetCorrectAnswer(Answer answer)
        {
            EditingQuestion.CorrectAnswerId = answer.Id;
        }

        private void CloseWithResult(bool result)
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
    }
}