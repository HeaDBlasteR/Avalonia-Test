using Avalonia;
using Avalonia.Headless;
using AvaloniaTests.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace AvaloniaTests.Tests
{
    // Runs the real App (styles, theme, resources) on the headless platform for [AvaloniaFact] tests.
    public class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
