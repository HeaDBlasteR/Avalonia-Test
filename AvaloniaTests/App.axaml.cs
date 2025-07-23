using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaTests.Models;
using AvaloniaTests.Services;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AvaloniaTests
{
    public static class ServiceProvider
    {
        public static IServiceProvider Instance { get; private set; } = null!;

        //»нициализаци€ дл€ внедрени€ зависимостей в ViewModel
        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITestService, JsonTestService>();
            services.AddSingleton<IResultService, JsonResultService>();
            services.AddSingleton<IErrorDialogService, ErrorDialogService>();
            
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IDialogService, DialogService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<TestEditorViewModel>();
            services.AddTransient<TestRunnerViewModel>();
            services.AddTransient<ResultViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<TestEditorWindow>();
            services.AddTransient<TestRunnerWindow>();
            services.AddTransient<ResultWindow>();

            // ‘абрика дл€ создани€ TestRunnerViewModel с параметром Test
            services.AddTransient<Func<Test, IResultService, TestRunnerViewModel>>(sp =>
                (test, resultService) => new TestRunnerViewModel(test, resultService));

            // ‘абрика дл€ создани€ ResultViewModel с параметрами TestResult и Test
            services.AddTransient<Func<TestResult, Test, ResultViewModel>>(sp =>
                (result, test) => new ResultViewModel(result, test));

            Instance = services.BuildServiceProvider();
        }
    }

    //»нициализаци€ и запуск приложени€
    public class App : Application
    {
        public App()
        {
            ServiceProvider.Init();
        }

        //»нициализаци€ XAML
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        // «апускаетс€ после завершени€ инициализации приложени€, устанавливает главное окно
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.Instance.GetRequiredService<MainWindow>();
                
                // ”станавливаем режим завершени€ приложени€ при закрытии главного окна
                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}