using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;

namespace AvaloniaTests.Views
{
    public partial class ErrorDialogWindow : Window
    {
        public ErrorDialogWindow(ErrorDialogViewModel viewModel)
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