using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaTests.Views
{
    public partial class QuestionEditorWindow : Window
    {
        public QuestionEditorWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}