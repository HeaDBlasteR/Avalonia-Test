using AvaloniaTests.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AvaloniaTests.Services
{
    public class JsonResultService : IResultService
    {
        private List<TestResult> _results = new();

        public JsonResultService()
        {
            LoadResults();
        }

        public List<TestResult> GetResults() => _results;

        public void SaveResult(TestResult result)
        {
            _results.Add(result);
            SaveResults();
        }

        private void LoadResults()
        {
            var resultsFilePath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\bin\Debug\net8.0\results.json";
            
            if (!File.Exists(resultsFilePath))
            {
                var directory = Path.GetDirectoryName(resultsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _results = new List<TestResult>();
                SaveResults();
                return;
            }

            var json = File.ReadAllText(resultsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _results = new List<TestResult>();
                return;
            }

            var loaded = JsonSerializer.Deserialize<List<TestResult>>(json);
            if (loaded != null)
            {
                foreach (var result in loaded)
                {
                    result.FixCollections();
                }
                _results = loaded;
            }
            else
            {
                _results = new List<TestResult>();
            }
        }

        private void SaveResults()
        {
            var resultsFilePath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\bin\Debug\net8.0\results.json";
            var json = JsonSerializer.Serialize(_results);
            File.WriteAllText(resultsFilePath, json);
        }
    }
}