using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;

namespace AgentFlow.Application.Outbox;

/// <summary>
/// Processes pending outbox messages and publishes them using the configured message publisher.
/// </summary>
public sealed class OutboxProcessor
{
    /// <summary>
    /// Repository used to retrieve and store outbox messages.
    /// </summary>
    private readonly IOutboxMessageRepository _outboxRepository;

    /// <summary>
    /// Publishes messages to an external message transport.
    /// </summary>
    private readonly IMessagePublisher _messagePublisher;

    /// <summary>
    /// Unit of work used to persist outbox state changes.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="OutboxProcessor"/>.
    /// </summary>
    public OutboxProcessor(
        IOutboxMessageRepository outboxRepository,
        IMessagePublisher messagePublisher,
        IUnitOfWork unitOfWork)
    {
        _outboxRepository = outboxRepository;
        _messagePublisher = messagePublisher;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processes all pending outbox messages and marks them as published.
    /// </summary>
    public async Task ProcessPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var messages = await _outboxRepository.GetPendingAsync(
            cancellationToken);

        foreach (var message in messages)
        {
            await _messagePublisher.PublishAsync(
                message.Type,
                message.Payload,
                cancellationToken);

            message.MarkPublished();

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }
    }
}
