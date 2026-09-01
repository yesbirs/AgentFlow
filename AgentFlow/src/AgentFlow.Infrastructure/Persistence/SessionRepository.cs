using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence
{
    public sealed class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _dbContext;

        public SessionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        {
            await _dbContext.Sessions.AddAsync(session, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        }

        public async Task<IReadOnlyList<Session>> GetSessions(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Sessions.ToListAsync();
        }

        public async Task<bool> IsExistAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Sessions.AnyAsync(s => s.Id == sessionId, cancellationToken);
        }
    }
}