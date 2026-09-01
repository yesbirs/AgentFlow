using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Domain.Entities
{
    public class Session
    {
        public Guid Id { get; private set; }
        public string Prompt { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        private readonly List<AgentTask> _tasks = new();

        public IReadOnlyCollection<AgentTask> Tasks => _tasks.AsReadOnly();

        private Session()
        { }

        public Session(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));
            }
            Id = Guid.NewGuid();
            Prompt = prompt;
            CreatedAt = DateTime.UtcNow;
        }
    }
}