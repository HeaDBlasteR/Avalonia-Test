using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AvaloniaTests.Models
{
    public class Question : INotifyPropertyChanged
    {
        private string _text = "";
        private Guid _correctAnswerId = Guid.Empty;

        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("Text")]
        public string Text 
        { 
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }
        
        [JsonPropertyName("Answers")]
        public List<Answer> AnswersData { get; set; }
        
        [JsonIgnore]
        public ObservableCollection<Answer> Answers { get; set; }
        
        [JsonPropertyName("CorrectAnswerId")]
        public Guid CorrectAnswerId 
        { 
            get => _correctAnswerId;
            set
            {
                _correctAnswerId = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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
            Answers = new ObservableCollection<Answer>(AnswersData);
            
            foreach (var answer in Answers)
            {
                if (answer.Id == Guid.Empty)
                    answer.Id = Guid.NewGuid();
            }
            
            AnswersData = Answers.ToList();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}