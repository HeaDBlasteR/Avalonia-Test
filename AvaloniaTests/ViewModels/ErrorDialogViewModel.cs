using ReactiveUI;
using System;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class ErrorDialogViewModel : ViewModelBase
    {
        public string Title { get; }
        public string Message { get; }
        public string ErrorIcon => "?";

        public ICommand OkCommand { get; private set; }

        public event EventHandler? CloseRequested;

        public ErrorDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            OkCommand = ReactiveCommand.Create(RequestClose);
        }

        private void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}