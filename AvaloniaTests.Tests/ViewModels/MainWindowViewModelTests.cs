using AvaloniaTests.Models;
using AvaloniaTests.Tests.Fakes;
using AvaloniaTests.ViewModels;
using System;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        private readonly Test _test = SampleData.CreateCapitalsTest();
        private readonly FakeTestService _testService;
        private readonly FakeResultService _resultService;
        private readonly FakeWindowService _windowService = new();

        public MainWindowViewModelTests()
        {
            _testService = new FakeTestService(_test);
            _resultService = new FakeResultService(SampleData.CreateResult(_test, 2, new DateTime(2025, 1, 10)));
        }

        private MainWindowViewModel CreateViewModel() =>
            new(_testService, _resultService, new FakeErrorDialogService(), _windowService);

        [Fact]
        public void Constructor_LoadsTestsAndResults()
        {
            var viewModel = CreateViewModel();

            Assert.Same(_test, Assert.Single(viewModel.Tests));
            Assert.Same(_resultService.Results[0], Assert.Single(viewModel.Results));
        }

        [Theory]
        [InlineData(true, 2)]
        [InlineData(false, 1)]
        public void CreateTest_ReloadsTestsOnlyWhenEditorWasSaved(bool saved, int expectedTestCount)
        {
            var viewModel = CreateViewModel();
            _windowService.TestEditorResult = saved;
            _testService.Tests.Add(SampleData.CreateTest("Арифметика", SampleData.CreateQuestion("2 + 2 = ?", 0, "4", "5")));

            viewModel.CreateTestCommand.Execute(null);

            Assert.Null(Assert.Single(_windowService.EditedTests));
            Assert.Equal(expectedTestCount, viewModel.Tests.Count);
        }

        [Fact]
        public void EditTest_OpensEditorForThatTest()
        {
            var viewModel = CreateViewModel();

            viewModel.EditTestCommand.Execute(_test);

            Assert.Same(_test, Assert.Single(_windowService.EditedTests));
        }

        [Fact]
        public void DeleteTest_DeletesTestAndReloads()
        {
            var viewModel = CreateViewModel();

            viewModel.DeleteTestCommand.Execute(_test);

            Assert.Equal(new[] { _test.Id }, _testService.DeletedIds);
            Assert.Empty(viewModel.Tests);
        }

        [Fact]
        public void TakeTest_ReloadsResultsAfterTestIsFinished()
        {
            var viewModel = CreateViewModel();
            _windowService.TestRunnerResult = true;
            _resultService.Results.Add(SampleData.CreateResult(_test, 3, new DateTime(2025, 1, 11)));

            viewModel.TakeTestCommand.Execute(_test);

            Assert.Same(_test, Assert.Single(_windowService.RunTests));
            Assert.Equal(2, viewModel.Results.Count);
        }

        [Fact]
        public void ViewResult_OpensViewerWithTheMatchingTest()
        {
            var viewModel = CreateViewModel();
            var result = viewModel.Results[0];

            viewModel.ViewResultsCommand.Execute(result);

            var viewed = Assert.Single(_windowService.ViewedResults);
            Assert.Same(result, viewed.Result);
            Assert.Same(_test, viewed.Test);
        }

        [Fact]
        public void MenuButtons_OpenTheirWindows()
        {
            var viewModel = CreateViewModel();

            viewModel.StartTestCommand.Execute(null);
            viewModel.OpenTestListCommand.Execute(null);
            viewModel.OpenResultsTabCommand.Execute(null);

            Assert.Equal(new[] { true, false }, _windowService.TestListRequests);
            Assert.Equal(1, _windowService.ResultsListRequests);
        }
    }
}
