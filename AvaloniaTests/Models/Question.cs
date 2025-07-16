using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace AvaloniaTests.Models
{
    public class Question
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("Text")]
        public string Text { get; set; }
        
        [JsonPropertyName("Answers")]
        public List<Answer> AnswersData { get; set; }
        
        [JsonIgnore]
        public ObservableCollection<Answer> Answers { get; set; }
        
        [JsonPropertyName("CorrectAnswerId")]
        public Guid CorrectAnswerId { get; set; }

        public Question()
        {
            Text = "";
            AnswersData = new List<Answer>();
            Answers = new ObservableCollection<Answer>();
            CorrectAnswerId = Guid.Empty;
        }

        public Question(string text)
        {
            Id = Guid.NewGuid();
            Text = text;
            AnswersData = new List<Answer>();
            Answers = new ObservableCollection<Answer>();
            CorrectAnswerId = Guid.Empty;
        }

        public void FixCollections()
        {
            if (AnswersData == null)
                AnswersData = new List<Answer>();
                
            Answers = new ObservableCollection<Answer>(AnswersData);
            
            foreach (var answer in Answers)
            {
                if (answer.Id == Guid.Empty)
                    answer.Id = Guid.NewGuid();
            }
            
            AnswersData = Answers.ToList();
        }
    }
}