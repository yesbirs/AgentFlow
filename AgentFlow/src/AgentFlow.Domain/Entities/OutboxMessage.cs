namespace AgentFlow.Domain.Entities;

/// <summary>
/// Represents a message stored in the outbox table used for reliable message delivery.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Unique identifier of the outbox message.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Message type used by consumers to determine the handler.
    /// </summary>
    public string Type { get; private set; } = string.Empty;

    /// <summary>
    /// Serialized payload of the message.
    /// </summary>
    public string Payload { get; private set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the outbox message was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp when the message was published, or null if not yet published.
    /// </summary>
    public DateTime? PublishedAt { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core for materialization.
    /// </summary>
    private OutboxMessage()
    {
    }

    /// <summary>
    /// Creates a new outbox message with the specified type and payload.
    /// </summary>
    public OutboxMessage(
        string type,
        string payload)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException(
                "Type cannot be null or empty.",
                nameof(type));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentException(
                "Payload cannot be null or empty.",
                nameof(payload));
        }

        Id = Guid.NewGuid();
        Type = type;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the message as published by setting the PublishedAt timestamp.
    /// Throws if the message was already published.
    /// </summary>
    public void MarkPublished()
    {
        if (PublishedAt.HasValue)
        {
            throw new InvalidOperationException(
                "Outbox message has already been published.");
        }

        PublishedAt = DateTime.UtcNow;
    }
}
