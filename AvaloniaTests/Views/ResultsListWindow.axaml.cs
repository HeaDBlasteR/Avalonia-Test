using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;

namespace AvaloniaTests.Views
{
    public partial class ResultsListWindow : Window
    {
        public ResultsListWindow(ResultsListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Подписываемся на событие закрытия из ViewModel
            viewModel.CloseRequested += (sender, e) => Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}