using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="OutboxMessage"/> entities.
/// </summary>
public sealed class OutboxMessageRepository
    : IOutboxMessageRepository
{
    private readonly AppDbContext _dbContext;

    public OutboxMessageRepository(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds an outbox message to the context.
    /// </summary>
    public Task<OutboxMessage> AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.Add(message);

        return Task.FromResult(message);
    }

    /// <summary>
    /// Returns pending outbox messages ordered by creation time.
    /// </summary>
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .OrderBy(message => message.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
