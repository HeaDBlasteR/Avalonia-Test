using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaTests.Views
{
    public partial class ConfirmationDialogWindow : Window
    {
        public ConfirmationDialogWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}