using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.Services;
using AvaloniaTests.ViewModels;

namespace AvaloniaTests.Views
{
    public partial class TestRunnerWindow : Window
    {
        public TestRunnerWindow(TestRunnerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            viewModel.CloseRequested += (sender, result) => Close(result);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}