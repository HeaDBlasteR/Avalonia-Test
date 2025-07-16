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
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            _resultsFilePath = Path.Combine(appDataPath, "results.json");
            
            System.Diagnostics.Debug.WriteLine($"JsonResultService: Путь к файлу результатов: {_resultsFilePath}");
            
            if (!File.Exists(_resultsFilePath))
            {
                System.Diagnostics.Debug.WriteLine("JsonResultService: Файл в AppData не найден, ищем в других местах");
                
                var exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "";
                var appJsonPath = Path.Combine(exeDir, "results.json");
                
                if (File.Exists(appJsonPath))
                {
                    File.Copy(appJsonPath, _resultsFilePath, true);
                    System.Diagnostics.Debug.WriteLine($"JsonResultService: Скопирован файл из {appJsonPath}");
                }
                else
                {
                    var sourceJsonPath = Path.Combine(Environment.CurrentDirectory, "results.json");
                    if (File.Exists(sourceJsonPath))
                    {
                        File.Copy(sourceJsonPath, _resultsFilePath, true);
                        System.Diagnostics.Debug.WriteLine($"JsonResultService: Скопирован файл из {sourceJsonPath}");
                    }
                    else
                    {
                        var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "results.json");
                        if (File.Exists(projectPath))
                        {
                            File.Copy(projectPath, _resultsFilePath, true);
                            System.Diagnostics.Debug.WriteLine($"JsonResultService: Скопирован файл из {projectPath}");
                        }
                        else
                        {
                            var currentDir = Directory.GetCurrentDirectory();
                            var possiblePaths = new[]
                            {
                                Path.Combine(currentDir, "AvaloniaTests", "results.json"),
                                Path.Combine(currentDir, "results.json"),
                                @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\results.json"
                            };

                            bool found = false;
                            foreach (var path in possiblePaths)
                            {
                                if (File.Exists(path))
                                {
                                    File.Copy(path, _resultsFilePath, true);
                                    System.Diagnostics.Debug.WriteLine($"JsonResultService: Скопирован файл из {path}");
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                System.Diagnostics.Debug.WriteLine("JsonResultService: Файл результатов не найден нигде, создаем новый");
                                _results = new List<TestResult>();
                                SaveResults();
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("JsonResultService: Файл найден в AppData");
            }
            
            LoadResults();
            
            System.Diagnostics.Debug.WriteLine($"JsonResultService: Инициализация завершена. Загружено {_results.Count} результатов");
        }

        public List<TestResult> GetResults() 
        {
            System.Diagnostics.Debug.WriteLine($"JsonResultService.GetResults: Возвращаем {_results.Count} результатов");
            return new List<TestResult>(_results);
        }

        public void SaveResult(TestResult result)
        {
            System.Diagnostics.Debug.WriteLine($"JsonResultService.SaveResult: Сохраняем результат теста {result.TestId}");
            System.Diagnostics.Debug.WriteLine($"  - Пользователь: {result.UserName}");
            System.Diagnostics.Debug.WriteLine($"  - Счет: {result.Score}/{result.MaxScore}");
            System.Diagnostics.Debug.WriteLine($"  - Дата: {result.CompletionDate}");
            
            if (result.Id == Guid.Empty)
            {
                result.Id = Guid.NewGuid();
            }
            
            result.FixCollections();
            
            _results.Add(result);
            SaveResults();
            
            System.Diagnostics.Debug.WriteLine($"JsonResultService.SaveResult: Результат сохранен. Всего результатов: {_results.Count}");
        }

        public void DeleteResult(Guid resultId)
        {
            var result = _results.FirstOrDefault(r => r.Id == resultId);
            if (result != null)
            {
                _results.Remove(result);
                SaveResults();
                System.Diagnostics.Debug.WriteLine($"JsonResultService.DeleteResult: Результат {resultId} удален. Осталось результатов: {_results.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"JsonResultService.DeleteResult: Результат {resultId} не найден");
            }
        }

        private void LoadResults()
        {
            if (!File.Exists(_resultsFilePath))
            {
                var directory = Path.GetDirectoryName(_resultsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                _results = new List<TestResult>();
                SaveResults();
                return;
            }

            try
            {
                var json = File.ReadAllText(_resultsFilePath, System.Text.Encoding.UTF8);
                System.Diagnostics.Debug.WriteLine($"JsonResultService.LoadResults: Прочитано {json.Length} символов из файла");
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    System.Diagnostics.Debug.WriteLine("JsonResultService.LoadResults: Файл пустой");
                    _results = new List<TestResult>();
                    return;
                }

                var loadedResults = JsonSerializer.Deserialize<List<TestResult>>(json, JsonOptions);
                if (loadedResults != null)
                {
                    _results = loadedResults.Where(r => r != null).ToList();
                    foreach (var result in _results)
                    {
                        result.FixCollections();
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"JsonResultService.LoadResults: Успешно десериализовано {_results.Count} результатов");
                    foreach (var result in _results)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {result.UserName}: {result.Score}/{result.MaxScore} ({result.CompletionDate})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("JsonResultService.LoadResults: Десериализация вернула null");
                    _results = new List<TestResult>();
                }
                
                System.Diagnostics.Debug.WriteLine($"JsonResultService.LoadResults: Загружено {_results.Count} результатов");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JsonResultService.LoadResults: Ошибка загрузки: {ex.Message}");
                Console.WriteLine($"Ошибка загрузки результатов: {ex.Message}");
                _results = new List<TestResult>();
            }
        }

        private void SaveResults()
        {
            try
            {
                var directory = Path.GetDirectoryName(_resultsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                var json = JsonSerializer.Serialize(_results, JsonOptions);
                File.WriteAllText(_resultsFilePath, json, System.Text.Encoding.UTF8);
                
                System.Diagnostics.Debug.WriteLine($"JsonResultService.SaveResults: Сохранено в файл {_resultsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JsonResultService.SaveResults: Ошибка сохранения: {ex.Message}");
                Console.WriteLine($"Ошибка сохранения результатов: {ex.Message}");
            }
        }
    }
}