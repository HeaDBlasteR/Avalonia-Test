using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaTests.Views
{
    public partial class ResultsListWindow : Window
    {
        public ResultsListWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}