using AgentFlow.Domain.Enums;

namespace AgentFlow.Domain.Entities
{
    /// <summary>
    /// Represents a unit of work (task) that the agent will execute. Contains lifecycle methods to transition
    /// the task through states (Pending, Queued, Running, Succeeded, Failed, Cancelled).
    /// </summary>
    public class AgentTask
    {
        /// <summary>
        /// Unique identifier of the task.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Identifier of the session this task belongs to.
        /// </summary>
        public Guid SessionId { get; private set; }

        /// <summary>
        /// Optional parent task id when this task is a subtask.
        /// </summary>
        public Guid? ParentTaskId { get; private set; }

        /// <summary>
        /// Short title describing the task.
        /// </summary>
        public string Title { get; private set; } = string.Empty;

        /// <summary>
        /// Detailed description of the task.
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Current state of the task.
        /// </summary>
        public TaskState State { get; private set; }

        /// <summary>
        /// Number of execution attempts performed for this task.
        /// </summary>
        public int AttemptCount { get; private set; }

        /// <summary>
        /// UTC timestamp when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// UTC timestamp when the task was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Parameterless constructor required by EF Core for materialization.
        /// </summary>
        private AgentTask()
        { }

        /// <summary>
        /// Creates a new task instance for the given session.
        /// </summary>
        public AgentTask(Guid sessionId, string title, string description, Guid? parentTaskId = null)
        {
            if (sessionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "SessionId cannot be empty.",
                    nameof(sessionId));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Title cannot be null or empty.",
                    nameof(title));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException(
                    "Description cannot be null or empty.",
                    nameof(description));
            }

            Id = Guid.NewGuid();

            SessionId = sessionId;

            ParentTaskId = parentTaskId;

            Title = title;

            Description = description;

            State = TaskState.Pending;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Transitions the task from Pending to Queued.
        /// </summary>
        public void Queue()
        {
            EnsureState(TaskState.Pending);

            State = TaskState.Queued;

            Touch();
        }

        /// <summary>
        /// Marks the task as Running and increments the attempt count.
        /// </summary>
        public void Start()
        {
            EnsureState(TaskState.Queued);

            State = TaskState.Running;

            AttemptCount++;

            Touch();
        }

        /// <summary>
        /// Moves a failed task back to the queued state for retry.
        /// </summary>
        public void Retry()
        {
            EnsureState(TaskState.Failed);

            State = TaskState.Queued;

            Touch();
        }

        /// <summary>
        /// Marks the task as succeeded.
        /// </summary>
        public void Succeed()
        {
            EnsureState(TaskState.Running);

            State = TaskState.Succeeded;

            Touch();
        }

        /// <summary>
        /// Marks the task as failed.
        /// </summary>
        public void Fail()
        {
            EnsureState(TaskState.Running);

            State = TaskState.Failed;

            Touch();
        }

        /// <summary>
        /// Cancels the task when it is in a cancellable state.
        /// </summary>
        public void Cancel()
        {
            if (State != TaskState.Pending &&
                State != TaskState.Queued &&
                State != TaskState.Running)
            {
                throw new InvalidOperationException(
                    "Task cannot be cancelled from its current state.");
            }

            State = TaskState.Cancelled;

            Touch();
        }

        /// <summary>
        /// Verifies that the current state matches the expected state and throws when it does not.
        /// This helps enforce valid lifecycle transitions.
        /// </summary>
        private void EnsureState(TaskState expectedState)
        {
            if (State != expectedState)
            {
                throw new InvalidOperationException(
                    $"Task must be in {expectedState} state.");
            }
        }
        /// <summary>
        /// Recovers a task that has been running for too long and marks it as failed. This is used to handle stale executions.
        /// </summary>
        public void RecoverFromStaleExecution()
        {
            EnsureState(TaskState.Running);

            State = TaskState.Failed;
            Touch();
        }

        /// <summary>
        /// Updates the UpdatedAt timestamp to the current UTC time.
        /// </summary>
        private void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
