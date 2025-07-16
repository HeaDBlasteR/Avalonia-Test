using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using System;

namespace AvaloniaTests.Views
{
    public partial class ResultsListWindow : Window
    {
        public ResultsListWindow(ResultsListViewModel viewModel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: Начинаем инициализацию");
                InitializeComponent();
                DataContext = viewModel;
                
                // Передаем ссылку на это окно в ViewModel
                viewModel.SetCurrentWindow(this);
                
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: Инициализация завершена успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow: Ошибка инициализации: {ex}");
                Console.WriteLine($"Ошибка инициализации ResultsListWindow: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.InitializeComponent: Ошибка загрузки XAML: {ex}");
                Console.WriteLine($"Ошибка загрузки XAML: {ex.Message}");
                throw;
            }
        }
    }
}