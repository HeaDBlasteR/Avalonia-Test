using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace AvaloniaTests.Services
{
    public interface IErrorDialogService
    {
        void ShowError(string title, string message);
    }

    public class ErrorDialogService : IErrorDialogService
    {
        public void ShowError(string title, string message)
        {
            try
            {
                var errorWindow = new Window
                {
                    Title = title,
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false
                };

                errorWindow.Background = new Avalonia.Media.LinearGradientBrush
                {
                    StartPoint = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Relative),
                    EndPoint = new Avalonia.RelativePoint(1, 1, Avalonia.RelativeUnit.Relative),
                    GradientStops = 
                    {
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.Parse("#FFE5E5"), 0),
                        new Avalonia.Media.GradientStop(Avalonia.Media.Color.Parse("#FFCCCC"), 1)
                    }
                };

                var content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(30),
                    Spacing = 20,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                var titleText = new TextBlock
                {
                    Text = "?? " + title,
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D32F2F")),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var messageText = new TextBlock
                {
                    Text = message,
                    FontSize = 14,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#424242")),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 100,
                    Height = 35,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D32F2F")),
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White),
                    FontWeight = Avalonia.Media.FontWeight.SemiBold,
                    CornerRadius = new Avalonia.CornerRadius(8)
                };

                okButton.Click += (_, __) => errorWindow.Close();

                content.Children.Add(titleText);
                content.Children.Add(messageText);
                content.Children.Add(okButton);

                errorWindow.Content = content;
                
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    errorWindow.ShowDialog(mainWindow);
                }
                else
                {
                    errorWindow.Show();
                }
            }
            catch
            {
            }
        }

        private static Window? GetMainWindow()
        {
            try
            {
                return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            }
            catch
            {
                return null;
            }
        }
    }
}