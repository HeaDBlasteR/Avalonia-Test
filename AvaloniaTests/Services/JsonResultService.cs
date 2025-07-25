using AvaloniaTests.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AvaloniaTests.Services
{
    public class JsonResultService : IResultService
    {
        private const string RESULTS_FILE_NAME = "results.json";
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        private List<TestResult> _results = new();
        private readonly string _resultsFilePath;

        public JsonResultService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AvaloniaTests");
            Directory.CreateDirectory(appDataPath);
            
            _resultsFilePath = Path.Combine(appDataPath, RESULTS_FILE_NAME);
            
            if (!File.Exists(_resultsFilePath))
            {
                var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                var appJsonPath = Path.Combine(exeDir, RESULTS_FILE_NAME);
                
                if (File.Exists(appJsonPath))
                {
                    File.Copy(appJsonPath, _resultsFilePath, true);
                }
                else
                {
                    var sourceJsonPath = Path.Combine(Environment.CurrentDirectory, RESULTS_FILE_NAME);
                    if (File.Exists(sourceJsonPath))
                    {
                        File.Copy(sourceJsonPath, _resultsFilePath, true);
                    }
                    else
                    {
                        var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", RESULTS_FILE_NAME);
                        if (File.Exists(projectPath))
                        {
                            File.Copy(projectPath, _resultsFilePath, true);
                        }
                        else
                        {
                            var currentDir = Directory.GetCurrentDirectory();
                            var possiblePaths = new[]
                            {
                                Path.Combine(currentDir, "AvaloniaTests", RESULTS_FILE_NAME),
                                Path.Combine(currentDir, RESULTS_FILE_NAME)
                            };

                            bool found = false;
                            foreach (var path in possiblePaths)
                            {
                                if (File.Exists(path))
                                {
                                    File.Copy(path, _resultsFilePath, true);
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                _results = new List<TestResult>();
                                SaveResults();
                                return;
                            }
                        }
                    }
                }
            }
            
            LoadResults();
        }

        public List<TestResult> GetResults() 
        {
            return new List<TestResult>(_results);
        }

        public void SaveResult(TestResult result)
        {
            if (result.Id == Guid.Empty)
            {
                result.Id = Guid.NewGuid();
            }
            
            _results.Add(result);
            SaveResults();
        }

        public void DeleteResult(Guid resultId)
        {
            var result = _results.FirstOrDefault(r => r.Id == resultId);
            if (result != null)
            {
                _results.Remove(result);
                SaveResults();
            }
        }

        private void LoadResults()
        {
            if (!File.Exists(_resultsFilePath))
            {
                _results = new List<TestResult>();
                SaveResults();
                return;
            }

            var json = File.ReadAllText(_resultsFilePath, System.Text.Encoding.UTF8);
            
            if (string.IsNullOrWhiteSpace(json))
            {
                _results = new List<TestResult>();
                return;
            }

            var loadedResults = JsonSerializer.Deserialize<List<TestResult>>(json, JsonOptions);
            _results = loadedResults?.Where(r => r != null).ToList() ?? new List<TestResult>();
        }

        private void SaveResults()
        {
            var directory = Path.GetDirectoryName(_resultsFilePath);
            Directory.CreateDirectory(directory!);
            
            var json = JsonSerializer.Serialize(_results, JsonOptions);
            File.WriteAllText(_resultsFilePath, json, System.Text.Encoding.UTF8);
        }
    }
}