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

        //Регистрирует все необходимые сервисы и ViewModel
        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITestService, JsonTestService>();
            services.AddSingleton<IResultService, JsonResultService>();
            services.AddSingleton<IErrorDialogService, ErrorDialogService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<TestEditorViewModel>();
            services.AddTransient<TestRunnerViewModel>();
            services.AddTransient<ResultViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<TestEditorWindow>();
            services.AddTransient<TestRunnerWindow>();
            services.AddTransient<ResultWindow>();

            // Фабрика для создания TestRunnerViewModel с параметром Test
            services.AddTransient<Func<Test, TestRunnerViewModel>>(sp =>
                test => new TestRunnerViewModel(test, sp.GetRequiredService<IResultService>()));

            // Фабрика для создания ResultViewModel с параметрами TestResult и Test
            services.AddTransient<Func<TestResult, Test, ResultViewModel>>(sp =>
                (result, test) => new ResultViewModel(result, test));

            Instance = services.BuildServiceProvider();
        }
    }

    //Инициализация и запуск приложения
    public class App : Application
    {
        public App()
        {
            ServiceProvider.Init();
        }

        //Инициализация XAML
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        // Вызывается после завершения инициализации фреймворка, устанавливает главное окно
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.Instance.GetRequiredService<MainWindow>();
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}