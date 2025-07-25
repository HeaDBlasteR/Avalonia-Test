using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.Models;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaTests.Services
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private Window? GetMainWindow()
        {
            return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }

        public async Task<bool> ShowTestEditorAsync(Test? testToEdit = null)
        {
            var testService = _serviceProvider.GetRequiredService<ITestService>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var viewModel = new TestEditorViewModel(testService, dialogService, testToEdit);
            var window = new TestEditorWindow(viewModel);
            
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                return result == true;
            }
            
            window.Show();
            return false;
        }

        public async Task<bool> ShowTestRunnerAsync(Test test)
        {
            var resultService = _serviceProvider.GetRequiredService<IResultService>();
            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
            var currentUserName = Environment.UserName ?? "Пользователь";
            var viewModel = new TestRunnerViewModel(test, resultService, dialogService, currentUserName);
            var window = new TestRunnerWindow(viewModel);
            
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                return result == true;
            }
            
            window.Show();
            return false;
        }

        public async Task ShowResultViewerAsync(TestResult result, Test? test)
        {
            var viewModel = new ResultViewModel(result, test);
            var window = new ResultWindow(viewModel);
            
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

        public async Task<bool> ShowTestListAsync(bool selectMode = false)
        {
            var testService = _serviceProvider.GetRequiredService<ITestService>();
            var windowService = this; // Используем текущий экземпляр
            var resultService = selectMode ? _serviceProvider.GetRequiredService<IResultService>() : null;
            
            var window = new TestListWindow(testService, windowService, resultService, selectMode);
            
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                return result == true;
            }
            
            window.Show();
            return false;
        }

        public async Task<bool> ShowResultsListAsync()
        {
            var resultService = _serviceProvider.GetRequiredService<IResultService>();
            var testService = _serviceProvider.GetRequiredService<ITestService>();
            var windowService = this; // Используем текущий экземпляр
            
            var viewModel = new ResultsListViewModel(resultService, testService, windowService);
            var window = new ResultsListWindow(viewModel);
            
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await window.ShowDialog<bool?>(mainWindow);
                return result == true;
            }
            
            window.Show();
            return false;
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

        public void CloseCurrentWindow()
        {
            var windows = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
            var currentWindow = windows?.LastOrDefault(w => w.IsActive);
            currentWindow?.Close();
        }
    }
}