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

        //инициализация для создания экземпляров зависимостей в ViewModel
        public static void Init()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ITestService, JsonTestService>();
            services.AddSingleton<IResultService, JsonResultService>();
            services.AddSingleton<IErrorDialogService, ErrorDialogService>();
            
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IDialogService, DialogService>();

            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<MainWindow>();
            services.AddTransient<TestEditorWindow>();
            services.AddTransient<TestRunnerWindow>();
            services.AddTransient<ResultWindow>();

            services.AddTransient<Func<Test?, TestEditorViewModel>>(sp =>
            {
                var testService = sp.GetRequiredService<ITestService>();
                var dialogService = sp.GetRequiredService<IDialogService>();
                return (test) => new TestEditorViewModel(testService, dialogService, test);
            });

            services.AddTransient<Func<Test, TestRunnerViewModel>>(sp =>
            {
                var resultService = sp.GetRequiredService<IResultService>();
                var dialogService = sp.GetRequiredService<IDialogService>();
                return (test) => new TestRunnerViewModel(test, resultService, dialogService, Environment.UserName ?? "Пользователь");
            });

            services.AddTransient<Func<TestResult, Test?, ResultViewModel>>(sp =>
                (result, test) => new ResultViewModel(result, test));

            Instance = services.BuildServiceProvider();
        }
    }

    //инициализация в начале приложения
    public class App : Application
    {
        public App()
        {
            ServiceProvider.Init();
        }

        //инициализация XAML
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        // вызывается после полной инициализации приложения, устанавливает главное окно
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = ServiceProvider.Instance.GetRequiredService<MainWindow>();
                
                // установка режима завершения приложения при закрытии главного окна
                desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}