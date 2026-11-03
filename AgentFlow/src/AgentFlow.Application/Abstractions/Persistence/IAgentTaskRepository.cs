using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Abstractions.Persistence;

/// <summary>
/// Repository contract for managing <see cref="AgentTask"/> entities.
/// </summary>
public interface IAgentTaskRepository
{
    /// <summary>
    /// Adds a new agent task to the repository (tracked by the unit of work).
    /// </summary>
    Task AddAsync(AgentTask task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a task by its unique identifier.
    /// </summary>
    Task<AgentTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending tasks for a session.
    /// </summary>
    Task<IReadOnlyList<AgentTask>> GetPendingTasksAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to atomically claim a pending task by id.
    /// Returns true when the claim succeeded.
    /// </summary>
    Task<bool> TryClaimAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks that have been running longer than the specified cutoff time.
    /// </summary>
    /// <param name="cutoff"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyList<AgentTask>> GetStaleRunningTasksAsync(DateTime cutoff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks associated with a specific session.
    /// </summary>
    Task<IReadOnlyList<AgentTask>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
}