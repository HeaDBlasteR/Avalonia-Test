using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Services;

namespace AvaloniaTests.Views
{
    public partial class TestListWindow : Window
    {
        public TestListWindow(ITestService testService, IWindowService windowService)
            : this(testService, windowService, null, false) { }

        public TestListWindow(ITestService testService, IWindowService windowService, IResultService? resultService, bool selectMode = false)
        {
            InitializeComponent();
            var vm = new TestListViewModel(testService, windowService, selectMode, resultService);
            DataContext = vm;
            
            vm.CloseRequested += (sender, e) => Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
