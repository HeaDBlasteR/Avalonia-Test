using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaTests.Views
{
    public partial class TestRunnerWindow : Window
    {
        public TestRunnerWindow(TestRunnerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}