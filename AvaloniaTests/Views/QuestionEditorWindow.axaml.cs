using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;

namespace AvaloniaTests.Views
{
    public partial class QuestionEditorWindow : Window
    {
        public QuestionEditorWindow(QuestionEditorViewModel viewModel)
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