using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
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
            var viewModel = new TestCompletionDialogViewModel(result);
            var window = new TestCompletionDialogWindow(viewModel);

            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                await window.ShowDialog(mainWindow);
            }
            else
            {
                window.Show();
            }
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var viewModel = new ConfirmationDialogViewModel(title, message);
            var window = new ConfirmationDialogWindow(viewModel);

            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                return result == true;
            }

            return false;
        }
    }
}