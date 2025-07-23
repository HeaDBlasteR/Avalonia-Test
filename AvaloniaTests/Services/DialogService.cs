using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaTests.Models;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AvaloniaTests.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private Window? GetMainWindow()
        {
            return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }

        public async Task<Question?> ShowQuestionEditorAsync(Question? question = null)
        {
            var viewModel = new QuestionEditorViewModel(question);
            var window = new QuestionEditorWindow(viewModel);
            
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                return result == true ? viewModel.EditingQuestion : null;
            }
            
            window.Show();
            return null;
        }

        public async Task ShowTestCompletionDialogAsync(TestResult result)
        {
            var dialog = new Window
            {
                Title = "Завершен тест",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = new SolidColorBrush(Color.Parse("#F5F5DC")),
                Foreground = new SolidColorBrush(Colors.Black),
                CanResize = false
            };

            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 15
            };

            var titleText = new TextBlock
            {
                Text = "Тест завершен!",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            var scoreText = new TextBlock
            {
                Text = $"Ваш результат: {result.Score} из {result.MaxScore}",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            var percentageText = new TextBlock
            {
                Text = $"Процент: {(result.MaxScore > 0 ? (int)((double)result.Score / result.MaxScore * 100) : 0)}%",
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new SolidColorBrush(Color.Parse("#FF4CAF50")),
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeight.SemiBold
            };

            okButton.Click += (_, __) => dialog.Close();

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(scoreText);
            stackPanel.Children.Add(percentageText);
            stackPanel.Children.Add(okButton);

            dialog.Content = stackPanel;

            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                await dialog.ShowDialog(mainWindow);
            }
            else
            {
                dialog.Show();
            }
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = new SolidColorBrush(Color.Parse("#F5F5DC"))
            };

            var content = new StackPanel
            {
                Margin = new Thickness(30),
                Spacing = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#2C3E50")),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var messageText = new TextBlock
            {
                Text = message,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#424242")),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 15
            };

            var yesButton = new Button
            {
                Content = "Да",
                Width = 80,
                Height = 35,
                Background = new SolidColorBrush(Color.Parse("#27AE60")),
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeight.SemiBold,
                CornerRadius = new CornerRadius(8)
            };

            var noButton = new Button
            {
                Content = "Нет",
                Width = 80,
                Height = 35,
                Background = new SolidColorBrush(Color.Parse("#E74C3C")),
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeight.SemiBold,
                CornerRadius = new CornerRadius(8)
            };

            yesButton.Click += (_, __) => dialog.Close(true);
            noButton.Click += (_, __) => dialog.Close(false);

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);

            content.Children.Add(titleText);
            content.Children.Add(messageText);
            content.Children.Add(buttonPanel);

            dialog.Content = content;

            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await dialog.ShowDialog<bool?>(mainWindow);
                return result == true;
            }

            return false;
        }
    }
}