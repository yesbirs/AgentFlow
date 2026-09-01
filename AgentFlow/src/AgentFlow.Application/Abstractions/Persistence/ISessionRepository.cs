using AgentFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Abstractions.Persistence
{
    public interface ISessionRepository
    {
        Task AddAsync(Session session, CancellationToken cancellationToken = default);

        Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Session>> GetSessions(CancellationToken cancellationToken = default);

        Task<bool> IsExistAsync(Guid sessionId, CancellationToken cancellationToken = default);
    }
}