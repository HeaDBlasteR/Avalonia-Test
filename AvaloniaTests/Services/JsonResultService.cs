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
            // Устанавливаем путь к файлу результатов в папке проекта
            _resultsFilePath = @"C:\Users\nozdr\source\repos\TestsAvaloniaMVVM\AvaloniaTests\results.json";
            
            System.Diagnostics.Debug.WriteLine($"JsonResultService: Путь к файлу результатов: {_resultsFilePath}");
            
            // Убеждаемся, что папка существует
            var directory = Path.GetDirectoryName(_resultsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
                System.Diagnostics.Debug.WriteLine($"JsonResultService: Создана папка {directory}");
            }
            
            // Если файл не существует, создаем пустой
            if (!File.Exists(_resultsFilePath))
            {
                System.Diagnostics.Debug.WriteLine("JsonResultService: Файл результатов не найден, создаем новый");
                _results = new List<TestResult>();
                SaveResults();
            }
            else
            {
                LoadResults();
            }
        }

        public List<TestResult> GetResults() => new List<TestResult>(_results);

        public void SaveResult(TestResult result)
        {
            System.Diagnostics.Debug.WriteLine($"JsonResultService.SaveResult: Сохраняем результат теста {result.TestId}");
            System.Diagnostics.Debug.WriteLine($"  - Пользователь: {result.UserName}");
            System.Diagnostics.Debug.WriteLine($"  - Счет: {result.Score}/{result.MaxScore}");
            System.Diagnostics.Debug.WriteLine($"  - Дата: {result.CompletionDate}");
            
            // Убеждаемся, что результат имеет ID
            if (result.Id == Guid.Empty)
            {
                result.Id = Guid.NewGuid();
            }
            
            // Проверяем и исправляем коллекции
            result.FixCollections();
            
            _results.Add(result);
            SaveResults();
            
            System.Diagnostics.Debug.WriteLine($"JsonResultService.SaveResult: Результат сохранен. Всего результатов: {_results.Count}");
        }

        private void LoadResults()
        {
            try
            {
                var json = File.ReadAllText(_resultsFilePath, System.Text.Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
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
                }
                else
                {
                    _results = new List<TestResult>();
                }
                
                System.Diagnostics.Debug.WriteLine($"JsonResultService.LoadResults: Загружено {_results.Count} результатов");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JsonResultService.LoadResults: Ошибка загрузки: {ex.Message}");
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