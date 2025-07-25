using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTests.ViewModels;
using AvaloniaTests.Views;

namespace AvaloniaTests.Services
{
    public class ErrorDialogService : IErrorDialogService
    {
        public void ShowError(string title, string message)
        {
            var viewModel = new ErrorDialogViewModel(title, message);
            var window = new ErrorDialogWindow(viewModel);
            
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                window.ShowDialog(mainWindow);
            }
            else
            {
                window.Show();
            }
        }

        private static Window? GetMainWindow()
        {
            return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }
    }
}