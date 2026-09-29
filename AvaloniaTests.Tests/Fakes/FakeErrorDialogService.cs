using AvaloniaTests.Services;
using System.Collections.Generic;

namespace AvaloniaTests.Tests.Fakes
{
    internal sealed class FakeErrorDialogService : IErrorDialogService
    {
        public List<(string Title, string Message)> Errors { get; } = new();

        public void ShowError(string title, string message) => Errors.Add((title, message));
    }
}
