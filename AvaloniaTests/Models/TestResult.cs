using System;
using System.Collections.Generic;

namespace AvaloniaTests.Models
{
    public class TestResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TestId { get; set; }
        public string UserName { get; set; } = "";
        public DateTime CompletionDate { get; set; } = DateTime.Now;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public Dictionary<Guid, Guid> UserAnswers { get; set; } = new();
    }
}