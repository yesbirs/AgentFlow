using System.Text.Json;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Sessions.CreateSession;

public sealed class CreateSessionHandler
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSessionHandler(
        ISessionRepository sessionRepository,
        IOutboxMessageRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Session> Handle(
        CreateSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var session = new Session(command.Prompt);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Add the session.
            await _sessionRepository.AddAsync(
                session,
                cancellationToken);

            // 2. Create a durable planning message.
            var message = new PlanSessionMessage(session.Id);

            var outboxMessage = new OutboxMessage(
                "PlanSession",
                JsonSerializer.Serialize(message));

            await _outboxRepository.AddAsync(
                outboxMessage,
                cancellationToken);

            // 3. Persist both changes together.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return session;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                CancellationToken.None);

            throw;
        }
    }
}