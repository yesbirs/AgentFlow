using AgentFlow.Application.Execution.Context;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Abstractions.Execution;

public interface ITaskContextProvider
{
    Task<TaskContext> BuildAsync(AgentTask task, CancellationToken cancellationToken = default);
}