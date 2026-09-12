namespace AgentFlow.Application.Abstractions.Queue;

public interface IWorkItem
{
    Guid TaskId { get; }

    Task CompleteAsync(
        CancellationToken cancellationToken = default);

    Task AbandonAsync(
        CancellationToken cancellationToken = default);

    Task DeadLetterMessageAsync(
        CancellationToken cancellationToken = default);
}