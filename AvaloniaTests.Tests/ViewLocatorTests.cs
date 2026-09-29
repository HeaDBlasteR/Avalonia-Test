using AvaloniaTests.ViewModels;
using Xunit;

namespace AvaloniaTests.Tests
{
    public class ViewLocatorTests
    {
        [Fact]
        public void Match_AcceptsOnlyViewModels()
        {
            var locator = new ViewLocator();

            Assert.True(locator.Match(new ErrorDialogViewModel("Ошибка", "Сообщение")));
            Assert.False(locator.Match("not a view model"));
            Assert.False(locator.Match(null));
        }

        [Fact]
        public void Build_WithoutData_ReturnsNull()
        {
            Assert.Null(new ViewLocator().Build(null));
        }
    }
}
