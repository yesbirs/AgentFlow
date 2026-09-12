using AgentFlow.Application.Abstractions.Queue;
using Azure.Messaging.ServiceBus;

namespace AgentFlow.Infrastructure.Queue;

public sealed class AzureServiceBusWorkQueue : IWorkQueue
{
    private readonly ServiceBusSender _sender;
    private readonly ServiceBusReceiver _receiver;

    public AzureServiceBusWorkQueue(ServiceBusClient client)
    {
        _sender = client.CreateSender("agentflow-tasks");

        _receiver = client.CreateReceiver(
            "agentflow-tasks",
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });
    }

    /// <summary>
    /// Enqueues a task id onto the Service Bus queue.
    /// </summary>
    public async Task EnqueueAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var message = new ServiceBusMessage(taskId.ToString());

        await _sender.SendMessageAsync(
            message,
            cancellationToken);
    }

    /// <summary>
    /// Attempts to receive a work item from the Service Bus queue.
    /// </summary>
    public async Task<IWorkItem?> ReceiveAsync(
        CancellationToken cancellationToken = default)
    {
        var message = await _receiver.ReceiveMessageAsync(
            maxWaitTime: TimeSpan.FromSeconds(5),
            cancellationToken: cancellationToken);

        if (message is null)
        {
            return null;
        }

        return new AzureServiceBusWorkItem(
            _receiver,
            message);
    }
}
