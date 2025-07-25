using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;

namespace AvaloniaTests.Views
{
    public partial class TestCompletionDialogWindow : Window
    {
        public TestCompletionDialogWindow(TestCompletionDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            viewModel.CloseRequested += (sender, e) => Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}