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
            viewModel.SetCurrentWindow(this);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}