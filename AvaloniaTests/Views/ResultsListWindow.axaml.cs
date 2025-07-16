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
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: �������� �������������");
                InitializeComponent();
                DataContext = viewModel;
                
                // �������� ������ �� ��� ���� � ViewModel
                viewModel.SetCurrentWindow(this);
                
                System.Diagnostics.Debug.WriteLine("ResultsListWindow: ������������� ��������� �������");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow: ������ �������������: {ex}");
                Console.WriteLine($"������ ������������� ResultsListWindow: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"ResultsListWindow.InitializeComponent: ������ �������� XAML: {ex}");
                Console.WriteLine($"������ �������� XAML: {ex.Message}");
                throw;
            }
        }
    }
}