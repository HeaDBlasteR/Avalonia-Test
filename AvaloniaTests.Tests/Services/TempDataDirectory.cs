using System;
using System.IO;

namespace AvaloniaTests.Tests.Services
{
    // Unique folder per test so the JSON services never touch the real %APPDATA%\AvaloniaTests data.
    internal sealed class TempDataDirectory : IDisposable
    {
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "AvaloniaTests.Tests", Guid.NewGuid().ToString("N"));

        public string FilePath(string fileName) => Path.Combine(DirectoryPath, fileName);

        public void WriteFile(string fileName, string content)
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(FilePath(fileName), content);
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
    }
}
