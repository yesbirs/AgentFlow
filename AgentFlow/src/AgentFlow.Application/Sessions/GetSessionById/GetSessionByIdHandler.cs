using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Sessions.GetSessionById;

public sealed class GetSessionByIdHandler
{
    private readonly ISessionRepository _sessionRepository;

    public GetSessionByIdHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Session?> Handle(GetSessionByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        return await _sessionRepository.GetSessionByIdAsync(query.SessionId, cancellationToken);
    }
}