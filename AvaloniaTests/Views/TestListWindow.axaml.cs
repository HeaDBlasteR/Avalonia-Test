using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
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
            var vm = new TestListViewModel(testService, selectMode, resultService);
            DataContext = vm;
            
            // Подписываемся на событие закрытия из ViewModel
            vm.CloseRequested += (sender, e) => Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
