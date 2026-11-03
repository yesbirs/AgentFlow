using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

public sealed class SessionResultRepository : ISessionResultRepository
{
    private readonly AppDbContext _dbContext;

    public SessionResultRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        SessionResult result,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SessionResults.AddAsync(
            result,
            cancellationToken);
    }

    public async Task<SessionResult?> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SessionResults
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.SessionId == sessionId,
                cancellationToken);
    }
}