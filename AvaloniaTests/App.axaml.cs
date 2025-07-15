using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace AvaloniaTests
{
    public static class ServiceProvider
    {
        public static IServiceProvider Instance { get; private set; } = null!;

        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITestService, JsonTestService>();
            services.AddSingleton<IResultService, JsonResultService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<TestEditorViewModel>();
            services.AddTransient<TestRunnerViewModel>();
            services.AddTransient<ResultViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<TestEditorWindow>();
            services.AddTransient<TestRunnerWindow>();
            services.AddTransient<ResultWindow>();

            services.AddTransient<Func<Test, TestRunnerViewModel>>(sp =>
                test => new TestRunnerViewModel(test, sp.GetRequiredService<IResultService>()));

            services.AddTransient<Func<TestResult, Test, ResultViewModel>>(sp =>
                (result, test) => new ResultViewModel(result, test));

            Instance = services.BuildServiceProvider();
        }
    }

    public class App : Application
    {
        public App()
        {
            ServiceProvider.Init();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            CheckTestsJsonFile();
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.Instance.GetRequiredService<MainWindow>();
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void CheckTestsJsonFile()
        {
            var testsJsonPath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\bin\Debug\net8.0\tests.json";
            
            if (!File.Exists(testsJsonPath))
            {
                var directory = Path.GetDirectoryName(testsJsonPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var sourceJsonPath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\tests.json";
                
                if (File.Exists(sourceJsonPath))
                {
                    File.Copy(sourceJsonPath, testsJsonPath, true);
                }
                else
                {
                    var emptyTestsJson = "[]";
                    File.WriteAllText(testsJsonPath, emptyTestsJson);
                }
            }
        }
    }
}