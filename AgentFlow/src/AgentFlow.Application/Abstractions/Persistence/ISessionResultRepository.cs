using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Abstractions.Persistence;

public interface ISessionResultRepository
{
    Task AddAsync(
        SessionResult result,
        CancellationToken cancellationToken = default);

    Task<SessionResult?> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}