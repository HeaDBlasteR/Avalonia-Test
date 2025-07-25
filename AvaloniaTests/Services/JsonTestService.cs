using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AvaloniaTests.Services
{
    public class JsonTestService : ITestService
    {
        private const string TESTS_FILE_NAME = "tests.json";
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        private List<Test> _tests = new();
        private readonly string _testFilePath;

        public JsonTestService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AvaloniaTests");
            Directory.CreateDirectory(appDataPath);
            
            _testFilePath = Path.Combine(appDataPath, TESTS_FILE_NAME);
            
            if (!File.Exists(_testFilePath))
            {
                var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                var appJsonPath = Path.Combine(exeDir, TESTS_FILE_NAME);
                
                if (File.Exists(appJsonPath))
                {
                    File.Copy(appJsonPath, _testFilePath, true);
                }
                else
                {
                    var sourceJsonPath = Path.Combine(Environment.CurrentDirectory, TESTS_FILE_NAME);
                    if (File.Exists(sourceJsonPath))
                    {
                        File.Copy(sourceJsonPath, _testFilePath, true);
                    }
                    else
                    {
                        var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", TESTS_FILE_NAME);
                        if (File.Exists(projectPath))
                        {
                            File.Copy(projectPath, _testFilePath, true);
                        }
                        else
                        {
                            _tests = new List<Test>();
                            SaveTests();
                            return;
                        }
                    }
                }
            }
            
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
            if (!File.Exists(_testFilePath))
            {
                _tests = new List<Test>();
                SaveTests();
                return;
            }

            var json = File.ReadAllText(_testFilePath, System.Text.Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                _tests = new List<Test>();
                return;
            }

            var loadedTests = JsonSerializer.Deserialize<List<Test>>(json, JsonOptions);
            _tests = loadedTests?.Where(t => t != null && !string.IsNullOrWhiteSpace(t.Title)).ToList() ?? new List<Test>();
            
            foreach (var test in _tests)
            {
                test.FixCollections();
            }
        }

        private void SaveTests()
        {
            var json = JsonSerializer.Serialize(_tests, JsonOptions);
            File.WriteAllText(_testFilePath, json, System.Text.Encoding.UTF8);
        }
    }
}