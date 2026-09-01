using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Sessions.CreateSession;

public sealed class CreateSessionHandler
{
    private readonly ISessionRepository _sessionRepository;

    public CreateSessionHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Session> Handle(
        CreateSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var session = new Session(command.Prompt);

        await _sessionRepository.AddAsync(
            session,
            cancellationToken);

        return session;
    }
}