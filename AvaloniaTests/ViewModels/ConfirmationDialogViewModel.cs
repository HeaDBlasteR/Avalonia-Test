using ReactiveUI;
using System;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class ConfirmationDialogViewModel : ViewModelBase
    {
        public string Title { get; }
        public string Message { get; }

        public ICommand YesCommand { get; private set; }
        public ICommand NoCommand { get; private set; }

        public event EventHandler<bool>? CloseRequested;

        public ConfirmationDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            YesCommand = ReactiveCommand.Create(() => RequestClose(true));
            NoCommand = ReactiveCommand.Create(() => RequestClose(false));
        }

        private void RequestClose(bool result)
        {
            CloseRequested?.Invoke(this, result);
        }
    }
}