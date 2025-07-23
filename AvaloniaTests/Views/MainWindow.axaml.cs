using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaTests.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AvaloniaTests.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = AvaloniaTests.ServiceProvider.Instance.GetRequiredService<MainWindowViewModel>();
            
            Closed += OnMainWindowClosed;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnMainWindowClosed(object? sender, EventArgs e)
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown(0);
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}