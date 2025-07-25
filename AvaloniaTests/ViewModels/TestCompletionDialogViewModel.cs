using AvaloniaTests.Models;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace AvaloniaTests.ViewModels
{
    public class TestCompletionDialogViewModel : ViewModelBase
    {
        public TestResult Result { get; }

        public string TestCompletedMessage => "Тест завершен!";
        public string ScoreMessage => $"Ваш результат: {Result.Score} из {Result.MaxScore}";
        public string PercentageMessage => $"Процент: {(Result.MaxScore > 0 ? (int)((double)Result.Score / Result.MaxScore * 100) : 0)}%";

        public ICommand OkCommand { get; private set; }

        public event EventHandler? CloseRequested;

        public TestCompletionDialogViewModel(TestResult result)
        {
            Result = result;
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