using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Models;
using AvaloniaTests.Services;

namespace AvaloniaTests.Views
{
    public partial class TestListWindow : Window
    {
        public TestListWindow(ITestService testService)
            : this(testService, null, false) { }

        public TestListWindow(ITestService testService, IResultService? resultService, bool selectMode = false)
        {
            InitializeComponent();
            var vm = new TestListViewModel(testService, this, selectMode, resultService);
            DataContext = vm;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
