using AgentFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Abstractions.Persistence
{
    /// <summary>
    /// Repository contract for managing sessions.
    /// </summary>
    public interface ISessionRepository
    {
        /// <summary>
        /// Adds a new session to the repository.
        /// </summary>
        Task AddAsync(Session session, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a session by its identifier.
        /// </summary>
        Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all sessions.
        /// </summary>
        Task<IReadOnlyList<Session>> GetSessions(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether the session exists.
        /// </summary>
        Task<bool> IsExistAsync(Guid sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets session ids that have pending tasks.
        /// </summary>
        Task<IReadOnlyList<Guid>> GetSessionIdsWithPendingTasksAsync(CancellationToken cancellationToken = default);
    }
}
