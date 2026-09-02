using System.Collections.Concurrent;
using AgentFlow.Application.Abstractions.Queue;

namespace AgentFlow.Infrastructure.Queue;

public sealed class InMemoryWorkQueue : IWorkQueue
{
    private readonly ConcurrentQueue<Guid> _queue = new();

    public Task EnqueueAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(taskId);

        return Task.CompletedTask;
    }

    public Task<Guid> DequeueAsync(
        CancellationToken cancellationToken = default)
    {
        if (_queue.TryDequeue(out var taskId))
        {
            return Task.FromResult(taskId);
        }

        throw new InvalidOperationException("Queue is empty.");
    }
}