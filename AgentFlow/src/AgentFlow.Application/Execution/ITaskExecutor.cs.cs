using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Execution;

public interface ITaskExecutor
{
    Task ExecuteAsync(
        AgentTask task,
        CancellationToken cancellationToken = default);
}