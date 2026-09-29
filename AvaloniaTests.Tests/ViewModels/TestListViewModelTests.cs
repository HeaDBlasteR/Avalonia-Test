using AvaloniaTests.Models;
using AvaloniaTests.Tests.Fakes;
using AvaloniaTests.ViewModels;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class TestListViewModelTests
    {
        private readonly Test _test = SampleData.CreateCapitalsTest();
        private readonly FakeTestService _testService;
        private readonly FakeWindowService _windowService = new();

        public TestListViewModelTests()
        {
            _testService = new FakeTestService(_test);
        }

        [Fact]
        public void Constructor_LoadsTests()
        {
            var viewModel = new TestListViewModel(_testService, _windowService);

            Assert.Same(_test, Assert.Single(viewModel.Tests));
            Assert.False(viewModel.IsSelectMode);
        }

        [Fact]
        public void Refresh_ReloadsTestsFromService()
        {
            var viewModel = new TestListViewModel(_testService, _windowService);
            _testService.Tests.Add(SampleData.CreateTest("Арифметика", SampleData.CreateQuestion("2 + 2 = ?", 0, "4", "5")));

            viewModel.RefreshCommand.Execute(null);

            Assert.Equal(2, viewModel.Tests.Count);
        }

        [Fact]
        public void DeleteTest_RemovesTestFromList()
        {
            var viewModel = new TestListViewModel(_testService, _windowService);

            viewModel.DeleteTestCommand.Execute(_test);

            Assert.Contains(_test.Id, _testService.DeletedIds);
            Assert.Empty(viewModel.Tests);
        }

        [Fact]
        public void SelectTest_InSelectMode_RunsTestAndClosesListWhenFinished()
        {
            var viewModel = new TestListViewModel(_testService, _windowService, new FakeResultService(), selectMode: true);
            var closed = false;
            viewModel.CloseRequested += (_, _) => closed = true;
            _windowService.TestRunnerResult = true;

            viewModel.SelectTestCommand.Execute(_test);

            Assert.True(viewModel.IsSelectMode);
            Assert.Same(_test, Assert.Single(_windowService.RunTests));
            Assert.True(closed);
        }

        [Fact]
        public void SelectTest_OutsideSelectMode_DoesNotRunTest()
        {
            var viewModel = new TestListViewModel(_testService, _windowService);

            viewModel.SelectTestCommand.Execute(_test);

            Assert.Empty(_windowService.RunTests);
        }

        [Fact]
        public void Close_RaisesCloseRequested()
        {
            var viewModel = new TestListViewModel(_testService, _windowService);
            var closed = false;
            viewModel.CloseRequested += (_, _) => closed = true;

            viewModel.CloseCommand.Execute(null);

            Assert.True(closed);
        }
    }
}
