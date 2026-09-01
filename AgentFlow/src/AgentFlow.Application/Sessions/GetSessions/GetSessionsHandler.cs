using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Sessions.GetSessions
{
    public sealed class GetSessionsHandler
    {
        private readonly ISessionRepository _sessionRepositoy;

        public GetSessionsHandler(ISessionRepository sessionRepository)
        {
            _sessionRepositoy = sessionRepository;
        }

        public async Task<IReadOnlyList<Session>> Handle(GetSessionsQuery query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);

            return await _sessionRepositoy.GetSessions(cancellationToken);
        }
    }
}