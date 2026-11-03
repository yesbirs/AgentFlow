namespace AgentFlow.Domain.Entities;

public class SessionResult
{
    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private SessionResult()
    { }

    public SessionResult(Guid sessionId, string content)
    {
        if (sessionId == Guid.Empty)
            throw new ArgumentException("SessionId cannot be empty.");

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.");

        Id = Guid.NewGuid();
        SessionId = sessionId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }
}