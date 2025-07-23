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
            try
            {
                var testService = _serviceProvider.GetRequiredService<ITestService>();
                var viewModel = new TestEditorViewModel(testService, testToEdit);
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
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось открыть редактор тестов: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ShowTestRunnerAsync(Test test)
        {
            try
            {
                var resultService = _serviceProvider.GetRequiredService<IResultService>();
                var viewModel = new TestRunnerViewModel(test, resultService);
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
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось открыть тест: {ex.Message}");
                return false;
            }
        }

        public async Task ShowResultViewerAsync(TestResult result, Test? test)
        {
            try
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
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось открыть результат: {ex.Message}");
            }
        }

        public async Task<bool> ShowTestListAsync(bool selectMode = false)
        {
            try
            {
                var testService = _serviceProvider.GetRequiredService<ITestService>();
                var resultService = selectMode ? _serviceProvider.GetRequiredService<IResultService>() : null;
                
                var window = new TestListWindow(testService, resultService, selectMode);
                
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    var result = await window.ShowDialog<bool?>(mainWindow);
                    return result == true;
                }
                
                window.Show();
                return false;
            }
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось открыть список тестов: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ShowResultsListAsync()
        {
            try
            {
                var resultService = _serviceProvider.GetRequiredService<IResultService>();
                var testService = _serviceProvider.GetRequiredService<ITestService>();
                
                var mainWindow = GetMainWindow();
                var viewModel = new ResultsListViewModel(resultService, testService, mainWindow);
                var window = new ResultsListWindow(viewModel);
                
                if (mainWindow != null)
                {
                    var result = await window.ShowDialog<bool?>(mainWindow);
                    return result == true;
                }
                
                window.Show();
                return false;
            }
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось открыть список результатов: {ex.Message}");
                return false;
            }
        }

        public async Task<Question?> ShowQuestionEditorAsync(Question? question = null)
        {
            try
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
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось открыть редактор вопросов: {ex.Message}");
                return null;
            }
        }

        public void CloseCurrentWindow()
        {
            try
            {
                var windows = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows;
                var currentWindow = windows?.LastOrDefault(w => w.IsActive);
                currentWindow?.Close();
            }
            catch (Exception ex)
            {
                var errorService = _serviceProvider.GetRequiredService<IErrorDialogService>();
                errorService.ShowError("Ошибка", $"Не удалось закрыть окно: {ex.Message}");
            }
        }
    }
}