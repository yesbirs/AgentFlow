namespace AgentFlow.Application.Abstractions.Queue;

public interface IWorkQueue
{
    Task EnqueueAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<Guid> DequeueAsync(CancellationToken cancellationToken = default);
}