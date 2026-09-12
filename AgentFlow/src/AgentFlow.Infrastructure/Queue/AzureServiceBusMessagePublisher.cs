using AgentFlow.Application.Abstractions.Queue;
using Azure.Messaging.ServiceBus;

namespace AgentFlow.Infrastructure.Queue;

/// <summary>
/// Publishes messages to Azure Service Bus for the agentflow-tasks topic/queue.
/// </summary>
public sealed class AzureServiceBusMessagePublisher
    : IMessagePublisher
{
    private readonly ServiceBusSender _sender;

    public AzureServiceBusMessagePublisher(
        ServiceBusClient client)
    {
        _sender = client.CreateSender("agentflow-tasks");
    }

    /// <summary>
    /// Publishes a message with the provided type and payload.
    /// </summary>
    public async Task PublishAsync(
        string type,
        string payload,
        CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(payload);

        message.ApplicationProperties["Type"] = type;

        await _sender.SendMessageAsync(
            message,
            cancellationToken);
    }
}
