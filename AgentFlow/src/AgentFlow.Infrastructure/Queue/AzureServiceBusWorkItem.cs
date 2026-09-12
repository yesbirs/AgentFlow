using System.Text.Json;
using AgentFlow.Application.Abstractions.Queue;
using Azure.Messaging.ServiceBus;

namespace AgentFlow.Infrastructure.Queue;

internal sealed class AzureServiceBusWorkItem : IWorkItem
{
    private readonly ServiceBusReceiver _receiver;
    private readonly ServiceBusReceivedMessage _message;

    public Guid TaskId { get; }

    public AzureServiceBusWorkItem(
        ServiceBusReceiver receiver,
        ServiceBusReceivedMessage message)
    {
        _receiver = receiver;
        _message = message;

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var workItem = JsonSerializer.Deserialize<ExecuteTaskMessage>(
            message.Body.ToString(), options)
            ?? throw new InvalidOperationException(
                "Unable to deserialize ExecuteTask message.");

        TaskId = workItem.TaskId;
    }

    public async Task CompleteAsync(
        CancellationToken cancellationToken = default)
    {
        await _receiver.CompleteMessageAsync(
            _message,
            cancellationToken: cancellationToken);
    }

    public async Task AbandonAsync(
        CancellationToken cancellationToken = default)
    {
        await _receiver.AbandonMessageAsync(
            _message,
            cancellationToken: cancellationToken);
    }

    public async Task DeadLetterMessageAsync(
        CancellationToken cancellationToken = default)
    {
        await _receiver.DeadLetterMessageAsync(
            _message,
            cancellationToken: cancellationToken);
    }
}