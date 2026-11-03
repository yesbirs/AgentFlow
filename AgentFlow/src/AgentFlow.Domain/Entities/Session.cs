using AgentFlow.Domain.Enums;
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

        public SessionState State { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public string? FailureReason { get; private set; }

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
            State = SessionState.Created;
            UpdatedAt = CreatedAt;
        }

        public void StartPlanning()
        {
            if (State != SessionState.Created)
                throw new InvalidOperationException(
                    "Only newly created sessions can start planning.");

            State = SessionState.Planning;
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartExecution()
        {
            if (State != SessionState.Planning)
                throw new InvalidOperationException(
                    "Session must be planning before execution.");

            State = SessionState.Executing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartFinalization()
        {
            if (State != SessionState.Executing)
                throw new InvalidOperationException(
                    "Session must be executing before finalization.");

            State = SessionState.Finalizing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (State != SessionState.Finalizing)
                throw new InvalidOperationException(
                    "Session must be finalizing before completion.");

            State = SessionState.Completed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Fail(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Failure reason is required.");

            if (State == SessionState.Completed)
                throw new InvalidOperationException(
                    "A completed session cannot be failed.");

            State = SessionState.Failed;
            FailureReason = reason;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}