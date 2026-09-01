using AgentFlow.Domain.Enums;

namespace AgentFlow.Domain.Entities
{
    public class AgentTask
    {
        public Guid Id { get; private set; }

        public Guid SessionId { get; private set; }

        public Guid? ParentTaskId { get; private set; }

        public string Title { get; private set; } = string.Empty;

        public string Description { get; private set; } = string.Empty;

        public TaskState State { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        private AgentTask()
        { }

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

        public void Queue()
        {
            EnsureState(TaskState.Pending);

            State = TaskState.Queued;

            Touch();
        }

        public void Start()
        {
            EnsureState(TaskState.Queued);

            State = TaskState.Running;

            Touch();
        }

        public void Succeed()
        {
            EnsureState(TaskState.Running);

            State = TaskState.Succeeded;

            Touch();
        }

        public void Fail()
        {
            EnsureState(TaskState.Running);

            State = TaskState.Failed;

            Touch();
        }

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

        private void EnsureState(TaskState expectedState)
        {
            if (State != expectedState)
            {
                throw new InvalidOperationException(
                    $"Task must be in {expectedState} state.");
            }
        }

        private void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}