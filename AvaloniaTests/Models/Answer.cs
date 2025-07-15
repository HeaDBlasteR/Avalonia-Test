using System;
using System.Text.Json.Serialization;

namespace AvaloniaTests.Models
{
    public class Answer
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("Text")]
        public string Text { get; set; }

        public Answer()
        {
            Text = "";
        }

        public Answer(string text)
        {
            Id = Guid.NewGuid();
            Text = text ?? "";
        }
    }
}
