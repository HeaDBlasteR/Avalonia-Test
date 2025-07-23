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
            
            Closing += OnMainWindowClosing;
            Closed += OnMainWindowClosed;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = false;
                
                System.Diagnostics.Debug.WriteLine("������� ���� �����������...");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"������ ��� �������� �������� ����: {ex.Message}");
                e.Cancel = false;
            }
        }

        private void OnMainWindowClosed(object? sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("������� ���� �������. ��������� ����������...");
                
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                {
                    lifetime.Shutdown(0);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"����������� ������ ��� ���������� ����������: {ex.Message}");
                Environment.Exit(0);
            }
        }
    }
}