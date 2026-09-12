namespace AgentFlow.Domain.Entities;

/// <summary>
/// Represents a dependency relationship between two agent tasks.
/// </summary>
public sealed class TaskDependency
{
    /// <summary>
    /// Unique identifier of the dependency entity.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The task that has a dependency.
    /// </summary>
    public Guid TaskId { get; private set; }

    /// <summary>
    /// The task that the TaskId depends on.
    /// </summary>
    public Guid DependsOnTaskId { get; private set; }

    /// <summary>
    /// UTC timestamp when the dependency was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core for materialization.
    /// </summary>
    private TaskDependency()
    {
    }

    /// <summary>
    /// Creates a new TaskDependency linking taskId to dependsOnTaskId.
    /// </summary>
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
