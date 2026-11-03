namespace AgentFlow.Domain.Entities;

public class TaskResult
{
    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private TaskResult()
    {
    }

    public TaskResult(Guid taskId, string content)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException(
                "TaskId cannot be empty.",
                nameof(taskId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException(
                "Content cannot be null or empty.",
                nameof(content));

        Id = Guid.NewGuid();
        TaskId = taskId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }
}