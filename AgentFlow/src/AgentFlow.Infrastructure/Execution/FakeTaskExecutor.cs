using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Infrastructure.Execution;

/// <summary>
/// A fake executor used for development/testing that simulates work by delaying.
/// </summary>
public sealed class FakeTaskExecutor : ITaskExecutor
{
    /// <summary>
    /// Simulates executing the provided task.
    /// </summary>
    public async Task ExecuteAsync(
        AgentTask task,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine(
            $"Executing task: {task.Title}");

        await Task.Delay(
            TimeSpan.FromSeconds(5),
            cancellationToken);

        Console.WriteLine(
            $"Finished task: {task.Title}");
    }
}
