using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using System;

namespace AvaloniaTests.Views
{
    public partial class TestRunnerWindow : Window
    {
        public TestRunnerWindow(TestRunnerViewModel viewModel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TestRunnerWindow: Начинаем инициализацию");
                InitializeComponent();
                DataContext = viewModel;
                System.Diagnostics.Debug.WriteLine("TestRunnerWindow: Инициализация завершена успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestRunnerWindow: Ошибка инициализации: {ex}");
                Console.WriteLine($"Ошибка инициализации TestRunnerWindow: {ex.Message}");
                throw;
            }
        }

        private void InitializeComponent()
        {
            try
            {
                AvaloniaXamlLoader.Load(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TestRunnerWindow.InitializeComponent: Ошибка загрузки XAML: {ex}");
                Console.WriteLine($"Ошибка загрузки XAML: {ex.Message}");
                throw;
            }
        }
    }
}