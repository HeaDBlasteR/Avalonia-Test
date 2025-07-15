using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaTests.Views
{
    public partial class ResultWindow : Window
    {
        public ResultWindow(ResultViewModel viewModel)
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