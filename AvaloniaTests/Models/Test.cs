using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AvaloniaTests.Models
{
    public class Test : INotifyPropertyChanged
    {
        private string _title = "";
        private string _description = "";
        
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("Title")]
        public string Title 
        { 
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        
        [JsonPropertyName("Description")]
        public string Description 
        { 
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        
        [JsonPropertyName("Questions")]
        public List<Question> QuestionsData { get; set; }

        [JsonIgnore]
        public ObservableCollection<Question> Questions { get; set; }

        public string TestName => Title;

        public event PropertyChangedEventHandler? PropertyChanged;

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
            Questions = new ObservableCollection<Question>(QuestionsData);

            foreach (var question in Questions)
            {
                if (question.Id == Guid.Empty)
                    question.Id = Guid.NewGuid();
                    
                question.FixCollections();

                foreach (var answer in question.Answers)
                {
                    if (answer.Id == Guid.Empty)
                        answer.Id = Guid.NewGuid();
                }
            }
            
            QuestionsData = Questions.ToList();
            OnPropertyChanged(nameof(Questions));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}