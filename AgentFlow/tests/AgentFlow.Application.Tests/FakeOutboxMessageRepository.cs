using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

public sealed class FakeOutboxMessageRepository : IOutboxMessageRepository
{
    public List<OutboxMessage> Messages { get; } = [];

    public Task<OutboxMessage> AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        Messages.Add(message);
        return Task.FromResult(message);
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<OutboxMessage>>([]);
    }
}