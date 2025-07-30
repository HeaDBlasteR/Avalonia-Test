using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaTests.Views
{
    public partial class TestListWindow : Window
    {
        public TestListWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
