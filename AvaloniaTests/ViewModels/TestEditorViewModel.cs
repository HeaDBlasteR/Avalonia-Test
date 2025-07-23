using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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

        public ICommand SaveCommand { get; }
        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand AddAnswerCommand { get; }
        public ICommand RemoveAnswerCommand { get; }
        public ICommand EditQuestionCommand { get; }
        public ICommand SetCorrectAnswerCommand { get; }

        public TestEditorViewModel(ITestService testService, Test? testToEdit = null)
        {
            _testService = testService;
            _dialogService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IDialogService>(ServiceProvider.Instance);
            EditingTest = testToEdit ?? new Test("", "");

            SaveCommand = ReactiveCommand.Create(SafeSaveTest);
            AddQuestionCommand = ReactiveCommand.CreateFromTask(AddQuestionAsync);
            RemoveQuestionCommand = ReactiveCommand.Create<Question>(RemoveQuestion);
            AddAnswerCommand = ReactiveCommand.Create<Question>(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer);
            EditQuestionCommand = ReactiveCommand.CreateFromTask<Question>(EditQuestionAsync);
            SetCorrectAnswerCommand = ReactiveCommand.Create<object[]>(SetCorrectAnswer);
        }

        //Сохранение с валидацией   
        private void SafeSaveTest()
        {
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
            CloseTestEditorWindow();
        }

        private void CloseTestEditorWindow()
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

        private async Task AddQuestionAsync()
        {
            var question = await _dialogService.ShowQuestionEditorAsync();
            if (question != null)
            {
                EditingTest.Questions.Add(question);
                this.RaisePropertyChanged(nameof(EditingTest));
                this.RaisePropertyChanged(nameof(EditingTest.Questions));
            }
        }

        private void RemoveQuestion(Question question)
        {
            EditingTest.Questions.Remove(question);
        }

        private void AddAnswer(Question question)
        {
            question.Answers.Add(new Answer(""));
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
                    
                    this.RaisePropertyChanged(nameof(EditingTest));
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
                
                this.RaisePropertyChanged(nameof(EditingTest));
                this.RaisePropertyChanged(nameof(EditingTest.Questions));
            }
        }

        private void SetCorrectAnswer(object[] parameters)
        {
            if (parameters.Length == 2 && parameters[0] is Question question && parameters[1] is Answer answer)
            {
                question.CorrectAnswerId = answer.Id;
            }
        }
    }
}