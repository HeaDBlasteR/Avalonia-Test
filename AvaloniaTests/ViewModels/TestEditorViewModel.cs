using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive;

namespace AvaloniaTests.ViewModels
{
    public class TestEditorViewModel : ViewModelBase
    {
        private readonly ITestService _testService;
        private readonly IErrorDialogService _errorDialogService;
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
            _errorDialogService = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IErrorDialogService>(ServiceProvider.Instance);
            EditingTest = testToEdit ?? new Test("", "");

            SaveCommand = ReactiveCommand.Create(SafeSaveTest);
            AddQuestionCommand = ReactiveCommand.CreateFromTask(AddQuestionAsync);
            RemoveQuestionCommand = ReactiveCommand.Create<Question>(RemoveQuestion);
            AddAnswerCommand = ReactiveCommand.Create<Question>(AddAnswer);
            RemoveAnswerCommand = ReactiveCommand.Create<Answer>(RemoveAnswer);
            EditQuestionCommand = ReactiveCommand.Create<Question>(EditQuestion);
            SetCorrectAnswerCommand = ReactiveCommand.Create<object[]>(SetCorrectAnswer);

            SubscribeToCommandErrors(SaveCommand);
            SubscribeToCommandErrors(AddQuestionCommand);
            SubscribeToCommandErrors(RemoveQuestionCommand);
            SubscribeToCommandErrors(AddAnswerCommand);
            SubscribeToCommandErrors(RemoveAnswerCommand);
            SubscribeToCommandErrors(EditQuestionCommand);
            SubscribeToCommandErrors(SetCorrectAnswerCommand);
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

        private Window GetMainWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;
        }

        private void SafeSaveTest()
        {
            if (string.IsNullOrWhiteSpace(EditingTest.Title))
            {
                _errorDialogService.ShowError("Ошибка валидации", "Пожалуйста, введите название теста!");
                return;
            }

            EditingTest.QuestionsData = EditingTest.Questions?.ToList() ?? new System.Collections.Generic.List<Question>();
            
            foreach (var question in EditingTest.Questions ?? new System.Collections.ObjectModel.ObservableCollection<Question>())
            {
                if (question.Id == Guid.Empty)
                    question.Id = Guid.NewGuid();

                question.AnswersData = question.Answers?.ToList() ?? new System.Collections.Generic.List<Answer>();
                
                foreach (var answer in question.Answers ?? new System.Collections.ObjectModel.ObservableCollection<Answer>())
                {
                    if (answer.Id == Guid.Empty)
                        answer.Id = Guid.NewGuid();
                }
            }

            _testService.SaveTest(EditingTest);
            var currentWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows
                .FirstOrDefault(w => w.DataContext == this);
            currentWindow?.Close();
        }

        private Window CreateStylishDialog(string title, int width = 600, int height = 500)
        {
            var window = new Window
            {
                Title = title,
                Width = width,
                Height = height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F5F5DC"))
            };

            return window;
        }

        private async Task AddQuestionAsync()
        {
            var question = new Question("");
            question.Answers.Add(new Answer(""));
            question.Answers.Add(new Answer(""));
            question.CorrectAnswerId = question.Answers[0].Id;

            var answersPanel = new StackPanel { Spacing = 10 };

            void RefreshAnswersUI()
            {
                answersPanel.Children.Clear();
                for (int i = 0; i < question.Answers.Count; i++)
                {
                    var answer = question.Answers[i];
                    var answerContainer = new Border
                    {
                        Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                        CornerRadius = new Avalonia.CornerRadius(8),
                        Padding = new Avalonia.Thickness(15),
                        Margin = new Avalonia.Thickness(0, 5),
                        BorderThickness = new Avalonia.Thickness(1),
                        BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0E0E0"))
                    };

                    var answerContent = new StackPanel 
                    { 
                        Orientation = Avalonia.Layout.Orientation.Horizontal, 
                        Spacing = 10 
                    };

                    var radio = new RadioButton
                    {
                        IsChecked = answer.Id == question.CorrectAnswerId,
                        GroupName = "answersGroup",
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    radio.Checked += (_, __) => question.CorrectAnswerId = answer.Id;

                    var answerBox = new TextBox 
                    { 
                        Width = 300, 
                        Text = answer.Text,
                        Watermark = $"Вариант ответа {i + 1}...",
                        CornerRadius = new Avalonia.CornerRadius(6),
                        Padding = new Avalonia.Thickness(10),
                        BorderThickness = new Avalonia.Thickness(2),
                        BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#BDC3C7"))
                    };
                    answerBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = answer, Mode = BindingMode.TwoWay });

                    var removeBtn = new Button 
                    { 
                        Content = "🗑️",
                        IsEnabled = question.Answers.Count > 2,
                        Width = 35,
                        Height = 35,
                        CornerRadius = new Avalonia.CornerRadius(6),
                        Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E74C3C")),
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                        FontSize = 14,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    removeBtn.Click += (_, __) => {
                        question.Answers.Remove(answer);
                        if (question.CorrectAnswerId == answer.Id)
                            question.CorrectAnswerId = question.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
                        RefreshAnswersUI();
                    };

                    answerContent.Children.Add(radio);
                    answerContent.Children.Add(answerBox);
                    answerContent.Children.Add(removeBtn);
                    answerContainer.Child = answerContent;
                    answersPanel.Children.Add(answerContainer);
                }
            }

            RefreshAnswersUI();

            var addAnswerBtn = new Button 
            { 
                Content = "➕ Добавить вариант",
                Padding = new Avalonia.Thickness(15, 8),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3498DB")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 10)
            };
            addAnswerBtn.Click += (_, __) => {
                question.Answers.Add(new Answer(""));
                RefreshAnswersUI();
            };

            var window = CreateStylishDialog("📝 Добавление вопроса");

            var mainContent = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                CornerRadius = new Avalonia.CornerRadius(15),
                Padding = new Avalonia.Thickness(30),
                Margin = new Avalonia.Thickness(20),
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0E0E0"))
            };

            var contentStack = new StackPanel { Spacing = 20 };

            var titleSection = new StackPanel { Spacing = 10 };
            var questionLabel = new TextBlock 
            { 
                Text = "Текст вопроса:",
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = 16,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2C3E50"))
            };

            var textBox = new TextBox 
            { 
                Watermark = "Введите текст вопроса...",
                Height = 80,
                AcceptsReturn = true,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(15),
                BorderThickness = new Avalonia.Thickness(2),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#BDC3C7"))
            };
            textBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = question, Mode = BindingMode.TwoWay });

            titleSection.Children.Add(questionLabel);
            titleSection.Children.Add(textBox);

            var answersLabel = new TextBlock 
            { 
                Text = "Варианты ответов:",
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = 16,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2C3E50"))
            };

            var scrollViewer = new ScrollViewer
            {
                Height = 200,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = answersPanel
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Spacing = 15,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };

            var saveButton = new Button 
            { 
                Content = "💾 Сохранить",
                IsDefault = true,
                Padding = new Avalonia.Thickness(20, 10),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#27AE60")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };
            saveButton.Click += (_, __) => window.Close(true);

            var cancelButton = new Button 
            { 
                Content = "Отмена",
                IsCancel = true,
                Padding = new Avalonia.Thickness(20, 10),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#95A5A6")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };
            cancelButton.Click += (_, __) => window.Close(false);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(saveButton);

            contentStack.Children.Add(titleSection);
            contentStack.Children.Add(answersLabel);
            contentStack.Children.Add(scrollViewer);
            contentStack.Children.Add(addAnswerBtn);
            contentStack.Children.Add(buttonPanel);

            mainContent.Child = contentStack;
            window.Content = mainContent;

            var mainWindow = GetMainWindow();
            var result = await window.ShowDialog<bool?>(mainWindow);
            
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
            var answersPanel = new StackPanel { Spacing = 10 };

            void RefreshAnswersUI()
            {
                answersPanel.Children.Clear();
                for (int i = 0; i < question.Answers.Count; i++)
                {
                    var answer = question.Answers[i];
                    var answerContainer = new Border
                    {
                        Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                        CornerRadius = new Avalonia.CornerRadius(8),
                        Padding = new Avalonia.Thickness(15),
                        Margin = new Avalonia.Thickness(0, 5),
                        BorderThickness = new Avalonia.Thickness(1),
                        BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0E0E0"))
                    };

                    var answerContent = new StackPanel 
                    { 
                        Orientation = Avalonia.Layout.Orientation.Horizontal, 
                        Spacing = 10 
                    };

                    var radio = new RadioButton
                    {
                        IsChecked = answer.Id == question.CorrectAnswerId,
                        GroupName = "answersGroup",
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    radio.Checked += (_, __) => question.CorrectAnswerId = answer.Id;

                    var answerBox = new TextBox 
                    { 
                        Width = 300, 
                        Text = answer.Text,
                        Watermark = $"Вариант ответа {i + 1}...",
                        CornerRadius = new Avalonia.CornerRadius(6),
                        Padding = new Avalonia.Thickness(10),
                        BorderThickness = new Avalonia.Thickness(2),
                        BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#BDC3C7"))
                    };
                    answerBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = answer, Mode = BindingMode.TwoWay });

                    var removeBtn = new Button 
                    { 
                        Content = "🗑️",
                        IsEnabled = question.Answers.Count > 2,
                        Width = 35,
                        Height = 35,
                        CornerRadius = new Avalonia.CornerRadius(6),
                        Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E74C3C")),
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                        FontSize = 14,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    removeBtn.Click += (_, __) => {
                        question.Answers.Remove(answer);
                        if (question.CorrectAnswerId == answer.Id)
                            question.CorrectAnswerId = question.Answers.FirstOrDefault()?.Id ?? Guid.Empty;
                        RefreshAnswersUI();
                    };

                    answerContent.Children.Add(radio);
                    answerContent.Children.Add(answerBox);
                    answerContent.Children.Add(removeBtn);
                    answerContainer.Child = answerContent;
                    answersPanel.Children.Add(answerContainer);
                }
            }

            RefreshAnswersUI();

            var addAnswerBtn = new Button 
            { 
                Content = "➕ Добавить вариант",
                Padding = new Avalonia.Thickness(15, 8),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3498DB")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 10)
            };
            addAnswerBtn.Click += (_, __) => {
                question.Answers.Add(new Answer(""));
                RefreshAnswersUI();
            };

            var editWindow = CreateStylishDialog("✏️ Редактирование вопроса");

            var mainContent = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                CornerRadius = new Avalonia.CornerRadius(15),
                Padding = new Avalonia.Thickness(30),
                Margin = new Avalonia.Thickness(20),
                BorderThickness = new Avalonia.Thickness(1),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E0E0E0"))
            };

            var contentStack = new StackPanel { Spacing = 20 };

            var titleSection = new StackPanel { Spacing = 10 };
            var questionLabel = new TextBlock 
            { 
                Text = "Текст вопроса:",
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = 16,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2C3E50"))
            };

            var textBox = new TextBox 
            { 
                Watermark = "Введите текст вопроса...",
                Height = 80,
                AcceptsReturn = true,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(15),
                BorderThickness = new Avalonia.Thickness(2),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#BDC3C7"))
            };
            textBox.Bind(TextBox.TextProperty, new Binding("Text") { Source = question, Mode = BindingMode.TwoWay });

            titleSection.Children.Add(questionLabel);
            titleSection.Children.Add(textBox);

            var answersLabel = new TextBlock 
            { 
                Text = "Варианты ответов:",
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = 16,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#2C3E50"))
            };

            var scrollViewer = new ScrollViewer
            {
                Height = 200,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = answersPanel
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Spacing = 15,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };

            var saveButton = new Button 
            { 
                Content = "💾 Сохранить",
                IsDefault = true,
                Padding = new Avalonia.Thickness(20, 10),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#27AE60")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };
            saveButton.Click += (_, __) => editWindow.Close(true);

            var cancelButton = new Button 
            { 
                Content = "Отмена",
                IsCancel = true,
                Padding = new Avalonia.Thickness(20, 10),
                CornerRadius = new Avalonia.CornerRadius(8),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#95A5A6")),
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            };
            cancelButton.Click += (_, __) => editWindow.Close(false);

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(saveButton);

            contentStack.Children.Add(titleSection);
            contentStack.Children.Add(answersLabel);
            contentStack.Children.Add(scrollViewer);
            contentStack.Children.Add(addAnswerBtn);
            contentStack.Children.Add(buttonPanel);

            mainContent.Child = contentStack;
            editWindow.Content = mainContent;

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