namespace AgentFlow.Application.Abstractions.Queue;

public interface IMessagePublisher
{
    Task PublishAsync(
        string type,
        string payload,
        CancellationToken cancellationToken = default);
}