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
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: �������� �������������");
                InitializeComponent();
                
                _viewModel = viewModel;
                DataContext = viewModel;
                
                viewModel.SetCurrentWindow(this);
                
                // ������� ���������� �������� ����
                this.Loaded += OnWindowLoaded;
                
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: ������������� ��������� �������");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow: ������ �������������: {ex}");
                Console.WriteLine($"������ ������������� ResultsListWindow: {ex.Message}");
                throw;
            }
        }
        
        private void OnWindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ResultsListWindow.OnWindowLoaded: ���� ���������");
            
            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.OnWindowLoaded: ViewModel �������, Results.Count = {_viewModel.Results.Count}");
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.OnWindowLoaded: ResultsCount = {_viewModel.ResultsCount}");
                
                // �������� DataContext
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.OnWindowLoaded: DataContext = {DataContext?.GetType().Name}");
                
                // ������������� ������� ������ ����� ��������� ��������
                Dispatcher.UIThread.Post(() =>
                {
                    System.Diagnostics.Debug.WriteLine("ResultsListWindow.OnWindowLoaded: ������������� ��������� ������");
                    if (_viewModel.RefreshCommand.CanExecute(null))
                    {
                        _viewModel.RefreshCommand.Execute(null);
                    }
                }, DispatcherPriority.Background);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ResultsListWindow.OnWindowLoaded: ViewModel �� �������!");
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
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.InitializeComponent: ������ �������� XAML: {ex}");
                Console.WriteLine($"������ �������� XAML: {ex.Message}");
                throw;
            }
        }
    }
}