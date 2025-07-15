using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace AvaloniaTests.Models
{
    public class Test
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("Title")]
        public string Title { get; set; }
        
        [JsonPropertyName("Description")]
        public string Description { get; set; }
        
        [JsonPropertyName("Questions")]
        public List<Question> QuestionsData { get; set; }

        [JsonIgnore]
        public ObservableCollection<Question> Questions { get; set; }

        public string TestName => Title;

        public Test()
        {
            Title = "";
            Description = "";
            QuestionsData = new List<Question>();
            Questions = new ObservableCollection<Question>();
        }

        public Test(string title, string description)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            QuestionsData = new List<Question>();
            Questions = new ObservableCollection<Question>();
        }

        public void FixCollections()
        {
            // Не создаем новый ID, если он уже существует
            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
                
            Questions = new ObservableCollection<Question>(QuestionsData);
            QuestionsData = Questions.ToList();

            foreach (var question in Questions)
            {
                // Не создаем новый ID для вопроса, если он уже существует
                if (question.Id == Guid.Empty)
                    question.Id = Guid.NewGuid();
                    
                question.FixCollections();

                foreach (var answer in question.Answers)
                {
                    // Не создаем новый ID для ответа, если он уже существует
                    if (answer.Id == Guid.Empty)
                        answer.Id = Guid.NewGuid();
                }
            }
        }
    }
}