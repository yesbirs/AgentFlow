using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Scheduling;

public interface IScheduler
{
    Task<IReadOnlyList<AgentTask>> FindReadyTasksAsync(Guid sessionId, CancellationToken cancellationToken = default);
}