using AvaloniaTests.Models;
using AvaloniaTests.Tests.Fakes;
using AvaloniaTests.ViewModels;
using System;
using Xunit;

namespace AvaloniaTests.Tests.ViewModels
{
    public class ResultsListViewModelTests
    {
        private readonly Test _test = SampleData.CreateCapitalsTest();
        private readonly FakeWindowService _windowService = new();

        private ResultsListViewModel CreateViewModel(FakeResultService resultService, params Test[] tests) =>
            new(resultService, new FakeTestService(tests), _windowService);

        [Fact]
        public void LoadsResultsNewestFirstWithTestTitlesAndPercentages()
        {
            var older = SampleData.CreateResult(_test, 1, new DateTime(2025, 1, 10, 9, 30, 0), "anna");
            var newer = SampleData.CreateResult(_test, 3, new DateTime(2025, 2, 20, 18, 5, 0), "boris");

            var viewModel = CreateViewModel(new FakeResultService(older, newer), _test);

            Assert.Equal(2, viewModel.ResultsCount);
            Assert.False(viewModel.HasNoResults);

            var first = viewModel.Results[0];
            Assert.Same(newer, first.Result);
            Assert.Same(_test, first.Test);
            Assert.Equal("Столицы", first.TestTitle);
            Assert.Equal("boris", first.UserName);
            Assert.Equal("3/3", first.Score);
            Assert.Equal(100, first.Percentage);
            Assert.Equal("20.02.2025 18:05", first.CompletionDate);

            Assert.Equal(33, viewModel.Results[1].Percentage);
        }

        [Fact]
        public void ResultOfDeletedTest_ShowsPlaceholderTitle()
        {
            var orphan = SampleData.CreateResult(_test, 1, new DateTime(2025, 1, 10));

            var viewModel = CreateViewModel(new FakeResultService(orphan));

            var item = Assert.Single(viewModel.Results);
            Assert.Null(item.Test);
            Assert.Equal("Неизвестный тест", item.TestTitle);
        }

        [Fact]
        public void NoResults_ShowsEmptyState()
        {
            var viewModel = CreateViewModel(new FakeResultService(), _test);

            Assert.True(viewModel.HasNoResults);
            Assert.Equal(0, viewModel.ResultsCount);
        }

        [Fact]
        public void DeleteResult_RemovesItemAndUpdatesCounters()
        {
            var resultService = new FakeResultService(SampleData.CreateResult(_test, 2, new DateTime(2025, 1, 10)));
            var viewModel = CreateViewModel(resultService, _test);

            viewModel.DeleteResultCommand.Execute(viewModel.Results[0]);

            Assert.Empty(resultService.Results);
            Assert.Empty(viewModel.Results);
            Assert.Equal(0, viewModel.ResultsCount);
            Assert.True(viewModel.HasNoResults);
        }

        [Fact]
        public void Refresh_PicksUpNewResults()
        {
            var resultService = new FakeResultService();
            var viewModel = CreateViewModel(resultService, _test);
            resultService.Results.Add(SampleData.CreateResult(_test, 2, new DateTime(2025, 1, 10)));

            viewModel.RefreshCommand.Execute(null);

            Assert.Single(viewModel.Results);
            Assert.Equal(1, viewModel.ResultsCount);
        }

        [Fact]
        public void ViewResult_OpensResultViewer()
        {
            var result = SampleData.CreateResult(_test, 2, new DateTime(2025, 1, 10));
            var viewModel = CreateViewModel(new FakeResultService(result), _test);

            viewModel.ViewResultCommand.Execute(viewModel.Results[0]);

            var viewed = Assert.Single(_windowService.ViewedResults);
            Assert.Same(result, viewed.Result);
            Assert.Same(_test, viewed.Test);
        }
    }
}
