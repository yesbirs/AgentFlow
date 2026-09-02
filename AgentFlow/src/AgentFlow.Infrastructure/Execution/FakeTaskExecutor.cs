using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Infrastructure.Execution;

public sealed class FakeTaskExecutor : ITaskExecutor
{
    public async Task ExecuteAsync(
        AgentTask task,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Executing task: {task.Title}");

        await Task.Delay(
            TimeSpan.FromSeconds(1),
            cancellationToken);

        Console.WriteLine(
            $"Finished task: {task.Title}");
    }
}