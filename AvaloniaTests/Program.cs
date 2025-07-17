using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace AvaloniaTests
{
    //Точка входа
    internal sealed class Program
    {
        //Запуск
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Создание
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .UseReactiveUI()
                .LogToTrace();
    }
}