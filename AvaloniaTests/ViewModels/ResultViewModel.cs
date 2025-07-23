using AvaloniaTests.Models;

namespace AvaloniaTests.ViewModels
{
    // Окно результатов сразу после тестирования
    public class ResultViewModel : ViewModelBase
    {
        public TestResult Result { get; }
        public Test? Test { get; }

        public string TestTitle => Test?.Title ?? "Неизвестный тест";
        public string TestDescription => Test?.Description ?? "Описание недоступно";
        public string UserName => Result.UserName;
        public string Score => $"{Result.Score}/{Result.MaxScore}";
        public string CompletionDate => Result.CompletionDate.ToString("dd.MM.yyyy HH:mm");
        public int Percentage => Result.MaxScore > 0 ? (int)((double)Result.Score / Result.MaxScore * 100) : 0;

        public ResultViewModel(TestResult result, Test? test)
        {
            Result = result;
            Test = test;
        }
    }
}