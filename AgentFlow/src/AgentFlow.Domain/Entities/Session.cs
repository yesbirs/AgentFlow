using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Domain.Entities
{
    /// <summary>
    /// Represents a user session which groups related tasks produced by the planner.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Unique identifier of the session.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Original prompt that produced this session.
        /// </summary>
        public string Prompt { get; private set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the session was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        private readonly List<AgentTask> _tasks = new();

        /// <summary>
        /// Tasks associated with this session.
        /// </summary>
        public IReadOnlyCollection<AgentTask> Tasks => _tasks.AsReadOnly();

        /// <summary>
        /// Parameterless constructor required by EF Core for materialization.
        /// </summary>
        private Session()
        { }

        /// <summary>
        /// Creates a new session for the provided prompt.
        /// </summary>
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
