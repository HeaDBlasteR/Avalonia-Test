using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestEditorViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private Test _editingTest;

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
            EditingTest = testToEdit ?? new Test("", "");

            SaveCommand = ReactiveCommand.Create(SafeSaveTest);
            AddQuestionCommand = ReactiveCommand.CreateFromTask(AddQuestionAsync);
            RemoveQuestionCommand = ReactiveCommand.Create<Question>(RemoveQuestion);
            AddAnswerCommand = ReactiveCommand.Create<Question>(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer);
            EditQuestionCommand = ReactiveCommand.Create<Question>(EditQuestion);
            SetCorrectAnswerCommand = ReactiveCommand.Create<object[]>(SetCorrectAnswer);
        }

        private Window GetMainWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;
        }

        private void SafeSaveTest()
        {
            if (string.IsNullOrWhiteSpace(EditingTest.Title))
            {
                return;
            }

            if (EditingTest.Questions != null)
            {
                EditingTest.QuestionsData = EditingTest.Questions.ToList();
                foreach (var question in EditingTest.Questions)
                {
                    if (question.Id == Guid.Empty)
                        question.Id = Guid.NewGuid();

                    if (question.Answers != null)
                    {
                        question.AnswersData = question.Answers.ToList();
                        foreach (var answer in question.Answers)
                        {
                            if (answer.Id == Guid.Empty)
                                answer.Id = Guid.NewGuid();
                        }
                    }
                    else
                    {
                        question.AnswersData = new System.Collections.Generic.List<Answer>();
                    }
                }
            }
            else
            {
                EditingTest.QuestionsData = new System.Collections.Generic.List<Question>();
            }

            _testService.SaveTest(EditingTest);
            var currentWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows
                .FirstOrDefault(w => w.DataContext == this);
            currentWindow?.Close();
        }

        private async Task AddQuestionAsync()
        {
            var question = new Question("");
            question.Answers.Add(new Answer(""));
            question.Answers.Add(new Answer(""));
            question.CorrectAnswerId = question.Answers[0].Id;

            var answersPanel = new StackPanel { Spacing = 5 };

            void RefreshAnswersUI()
            {
                answersPanel.Children.Clear();
                foreach (var answer in question.Answers)
                {
                    var radio = new RadioButton
                    {
                        IsChecked = answer.Id == question.CorrectAnswerId,
                        GroupName = "answersGroup"
                    };
                    radio.Checked += (_, __) => question.CorrectAnswerId = answer.Id;

                    var answerBox = new TextBox { Width = 200, Text = answer.Text };
                    answerBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = answer, Mode = BindingMode.TwoWay });

                    var removeBtn = new Button { Content = "Удалить", IsEnabled = question.Answers.Count > 2, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
                    removeBtn.Click += (_, __) => {
                        question.Answers.Remove(answer);
                        if (question.CorrectAnswerId == answer.Id)
                            question.CorrectAnswerId = question.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
                        RefreshAnswersUI();
                    };

                    var answerPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 5 };
                    answerPanel.Children.Add(radio);
                    answerPanel.Children.Add(answerBox);
                    answerPanel.Children.Add(removeBtn);
                    answersPanel.Children.Add(answerPanel);
                }
            }

            RefreshAnswersUI();

            var addAnswerBtn = new Button { Content = "Добавить вариант", Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
            addAnswerBtn.Click += (_, __) => {
                question.Answers.Add(new Answer(""));
                RefreshAnswersUI();
            };

            var window = new Window
            {
                Title = "Добавление вопроса",
                Width = 500,
                Height = 400,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FFF5E6CC")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black)
            };

            var textBox = new TextBox { Watermark = "Введите текст вопроса" };
            textBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = question, Mode = BindingMode.TwoWay });

            var saveButton = new Button { Content = "Сохранить", IsDefault = true, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
            saveButton.Click += (_, __) => window.Close(true);
            var cancelButton = new Button { Content = "Отмена", IsCancel = true, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
            cancelButton.Click += (_, __) => window.Close(false);

            window.Content = new StackPanel
            {
                Margin = new Thickness(10),
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Текст вопроса:" },
                    textBox,
                    new TextBlock { Text = "Варианты ответа:" },
                    answersPanel,
                    addAnswerBtn,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 5,
                        Children = { saveButton, cancelButton }
                    }
                }
            };

            var result = await window.ShowDialog<bool?>(GetMainWindow());
            
            if (result == true && !string.IsNullOrWhiteSpace(question.Text))
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
                        question.CorrectAnswerId = Guid.Empty;
                    }
                    break;
                }
            }
        }

        private async void EditQuestion(Question question)
        {
            var answersPanel = new StackPanel { Spacing = 5 };

            void RefreshAnswersUI()
            {
                answersPanel.Children.Clear();
                foreach (var answer in question.Answers)
                {
                    var radio = new RadioButton
                    {
                        IsChecked = answer.Id == question.CorrectAnswerId,
                        GroupName = "answersGroup"
                    };
                    radio.Checked += (_, __) => question.CorrectAnswerId = answer.Id;

                    var answerBox = new TextBox { Width = 200, Text = answer.Text };
                    answerBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = answer, Mode = BindingMode.TwoWay });

                    var removeBtn = new Button { Content = "Удалить", IsEnabled = question.Answers.Count > 2, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
                    removeBtn.Click += (_, __) => {
                        question.Answers.Remove(answer);
                        if (question.CorrectAnswerId == answer.Id)
                            question.CorrectAnswerId = question.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
                        RefreshAnswersUI();
                    };

                    var answerPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 5 };
                    answerPanel.Children.Add(radio);
                    answerPanel.Children.Add(answerBox);
                    answerPanel.Children.Add(removeBtn);
                    answersPanel.Children.Add(answerPanel);
                }
            }

            RefreshAnswersUI();

            var addAnswerBtn = new Button { Content = "Добавить вариант", Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
            addAnswerBtn.Click += (_, __) => {
                question.Answers.Add(new Answer(""));
                RefreshAnswersUI();
            };

            var editWindow = new Window
            {
                Title = "Редактирование вопроса",
                Width = 500,
                Height = 400,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FFF5E6CC")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black)
            };

            var textBox = new TextBox { Watermark = "Введите текст вопроса" };
            textBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = question, Mode = BindingMode.TwoWay });

            var saveButton = new Button { Content = "Сохранить", IsDefault = true, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
            saveButton.Click += (_, __) => editWindow.Close(true);
            var cancelButton = new Button { Content = "Отмена", IsCancel = true, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Black) };
            cancelButton.Click += (_, __) => editWindow.Close(false);

            editWindow.Content = new StackPanel
            {
                Margin = new Thickness(10),
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Текст вопроса:" },
                    textBox,
                    new TextBlock { Text = "Варианты ответа:" },
                    answersPanel,
                    addAnswerBtn,
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Spacing = 5,
                        Children = { saveButton, cancelButton }
                    }
                }
            };

            var result = await editWindow.ShowDialog<bool?>(GetMainWindow());
            
            if (result == true)
            {
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