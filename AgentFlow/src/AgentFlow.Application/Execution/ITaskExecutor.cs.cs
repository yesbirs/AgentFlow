using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Execution;

/// <summary>
/// Executes a given <see cref="AgentTask"/>.
/// </summary>
public interface ITaskExecutor
{
    /// <summary>
    /// Executes the provided task instance.
    /// </summary>
    Task ExecuteAsync(
        AgentTask task,
        CancellationToken cancellationToken = default);
}
