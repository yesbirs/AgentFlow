namespace AgentFlow.Application.Abstractions.Queue;

public interface IWorkQueue
{
    Task EnqueueAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    Task<IWorkItem?> ReceiveAsync(
        CancellationToken cancellationToken = default);
}