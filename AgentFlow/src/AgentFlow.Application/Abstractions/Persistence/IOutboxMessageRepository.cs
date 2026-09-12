using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Abstractions.Persistence;

/// <summary>
/// Repository contract for storing and retrieving outbox messages used for reliable message publishing.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>
    /// Adds an outbox message to be published later.
    /// </summary>
    Task<OutboxMessage> AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves pending outbox messages that have not been published yet.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        CancellationToken cancellationToken = default);
}
