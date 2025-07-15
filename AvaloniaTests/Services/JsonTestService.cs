using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AvaloniaTests.Services
{
    public class JsonTestService : ITestService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        private List<Test> _tests = new();

        public JsonTestService()
        {
            LoadTests();
        }

        public List<Test> GetTests()
        {
            return new List<Test>(_tests);
        }

        public void SaveTest(Test test)
        {
            var existing = _tests.FirstOrDefault(t => t.Id == test.Id);
            var isNewTest = test.Id == Guid.Empty || existing == null;
            
            if (isNewTest)
            {
                test.Id = Guid.NewGuid();
            }

            test.QuestionsData = test.Questions.ToList();
            foreach (var question in test.Questions)
            {
                question.AnswersData = question.Answers.ToList();
            }
            
            test.FixCollections();
            
            if (existing != null)
            {
                _tests.Remove(existing);
            }
            
            _tests.Add(test);
            SaveTests();
        }

        public void DeleteTest(Guid testId)
        {
            var test = _tests.FirstOrDefault(t => t.Id == testId);
            if (test != null)
            {
                _tests.Remove(test);
                SaveTests();
            }
        }

        private void LoadTests()
        {
            var testFilePath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\bin\Debug\net8.0\tests.json";
            
            if (!File.Exists(testFilePath))
            {
                var directory = Path.GetDirectoryName(testFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                _tests = new List<Test>();
                SaveTests();
                return;
            }

            var json = File.ReadAllText(testFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _tests = new List<Test>();
                return;
            }

            var loadedTests = JsonSerializer.Deserialize<List<Test>>(json, JsonOptions);
            if (loadedTests != null)
            {
                _tests = loadedTests.Where(t => t != null && !string.IsNullOrWhiteSpace(t.Title)).ToList();
                
                foreach (var test in _tests)
                {
                    if (test.QuestionsData == null)
                        test.QuestionsData = new List<Question>();
                    
                    test.Questions = new ObservableCollection<Question>(test.QuestionsData);
                    
                    foreach (var question in test.Questions)
                    {
                        if (question.AnswersData == null)
                            question.AnswersData = new List<Answer>();
                        question.Answers = new ObservableCollection<Answer>(question.AnswersData);
                    }
                    test.FixCollections();
                }
            }
            else
            {
                _tests = new List<Test>();
            }
        }

        private void SaveTests()
        {
            var testFilePath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\bin\Debug\net8.0\tests.json";
            var json = JsonSerializer.Serialize(_tests, JsonOptions);
            File.WriteAllText(testFilePath, json);
        }
    }
}