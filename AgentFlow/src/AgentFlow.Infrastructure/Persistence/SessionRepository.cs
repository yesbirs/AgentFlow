using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.Infrastructure.Persistence
{
    /// <summary>
    /// Repository implementation for managing <see cref="Session"/> entities.
    /// </summary>
    public sealed class SessionRepository : ISessionRepository
    {
        private readonly AppDbContext _dbContext;

        public SessionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new session and persists the change immediately.
        /// </summary>
        public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
        {
            await _dbContext.Sessions.AddAsync(
                session,
                cancellationToken);
        }

        /// <summary>
        /// Retrieves a session by id.
        /// </summary>
        public async Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        }

        /// <summary>
        /// Returns all sessions.
        /// </summary>
        public async Task<IReadOnlyList<Session>> GetSessions(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Sessions.ToListAsync();
        }

        /// <summary>
        /// Checks if a session exists by id.
        /// </summary>
        public async Task<bool> IsExistAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Sessions.AnyAsync(s => s.Id == sessionId, cancellationToken);
        }

        /// <summary>
        /// Gets session ids which have pending tasks.
        /// </summary>
        public async Task<IReadOnlyList<Guid>> GetSessionIdsWithPendingTasksAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Tasks
                .Where(task => task.State == TaskState.Pending)
                .Select(task => task.SessionId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}