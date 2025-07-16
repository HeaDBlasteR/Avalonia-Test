using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using System;
using Avalonia.Threading;

namespace AvaloniaTests.Views
{
    public partial class ResultsListWindow : Window
    {
        private ResultsListViewModel? _viewModel;
        
        public ResultsListWindow(ResultsListViewModel viewModel)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: Начинаем инициализацию");
                InitializeComponent();
                
                _viewModel = viewModel;
                DataContext = viewModel;
                
                viewModel.SetCurrentWindow(this);
                
                // Добавим обработчик загрузки окна
                this.Loaded += OnWindowLoaded;
                
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: Инициализация завершена успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow: Ошибка инициализации: {ex}");
                Console.WriteLine($"Ошибка инициализации ResultsListWindow: {ex.Message}");
                throw;
            }
        }
        
        private void OnWindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ResultsListWindow.OnWindowLoaded: Окно загружено");
            
            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.OnWindowLoaded: ViewModel найдена, Results.Count = {_viewModel.Results.Count}");
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.OnWindowLoaded: ResultsCount = {_viewModel.ResultsCount}");
                
                // Проверим DataContext
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.OnWindowLoaded: DataContext = {DataContext?.GetType().Name}");
                
                // Принудительно обновим данные через небольшую задержку
                Dispatcher.UIThread.Post(() =>
                {
                    System.Diagnostics.Debug.WriteLine("ResultsListWindow.OnWindowLoaded: Принудительно обновляем данные");
                    if (_viewModel.RefreshCommand.CanExecute(null))
                    {
                        _viewModel.RefreshCommand.Execute(null);
                    }
                }, DispatcherPriority.Background);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ResultsListWindow.OnWindowLoaded: ViewModel не найдена!");
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