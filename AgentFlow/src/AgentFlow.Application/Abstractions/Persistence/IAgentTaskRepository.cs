using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Abstractions.Persistence;

public interface IAgentTaskRepository
{
    Task AddAsync(AgentTask task, CancellationToken cancellationToken = default);

    Task<AgentTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentTask>> GetPendingTasksAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<bool> TryClaimAsync(Guid taskId, CancellationToken cancellationToken = default);
}