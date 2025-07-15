using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaTests.Views
{
    public partial class TestEditorWindow : Window
    {
        public TestEditorWindow(TestEditorViewModel viewModel)
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