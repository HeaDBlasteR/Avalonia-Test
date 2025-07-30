using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaTests.Views
{
    public partial class TestCompletionDialogWindow : Window
    {
        public TestCompletionDialogWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}