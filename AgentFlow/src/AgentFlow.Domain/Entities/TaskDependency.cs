namespace AgentFlow.Domain.Entities;

public sealed class TaskDependency
{
    public Guid Id { get; private set; }

    public Guid TaskId { get; private set; }

    public Guid DependsOnTaskId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private TaskDependency()
    {
    }

    public TaskDependency(Guid taskId, Guid dependsOnTaskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException(
                "TaskId cannot be empty.",
                nameof(taskId));
        }

        if (dependsOnTaskId == Guid.Empty)
        {
            throw new ArgumentException(
                "DependsOnTaskId cannot be empty.",
                nameof(dependsOnTaskId));
        }

        if (taskId == dependsOnTaskId)
        {
            throw new InvalidOperationException(
                "A task cannot depend on itself.");
        }

        Id = Guid.NewGuid();

        TaskId = taskId;

        DependsOnTaskId = dependsOnTaskId;

        CreatedAt = DateTime.UtcNow;
    }
}