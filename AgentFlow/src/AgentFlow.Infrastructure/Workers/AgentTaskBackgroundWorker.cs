using AgentFlow.Application.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AgentFlow.Infrastructure.Workers;

/// <summary>
/// Background service that continuously processes available tasks by resolving a scoped TaskWorker.
/// </summary>
public sealed class AgentTaskBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AgentTaskBackgroundWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Background loop that continuously pulls work from the DI-scoped <see cref="TaskWorker"/>
    /// and executes one work item at a time.
    ///
    /// Behavior details:
    /// - Runs until the provided <paramref name="stoppingToken"/> is cancelled.
    /// - For each iteration it creates a new IServiceScope so scoped services (repositories, unit-of-work)
    ///   have an isolated lifetime per work attempt.
    /// - Resolves <see cref="TaskWorker"/> from the scope and calls <c>ProcessNextAsync</c> to
    ///   receive and execute a single work item. If no item is available the worker returns quickly.
    /// - Handles cancellation gracefully: when an OperationCanceledException is thrown because the
    ///   stopping token is signaled, the loop breaks and the service stops.
    /// - Logs and swallows other exceptions so the background loop continues running after transient errors.
    ///
    /// This design keeps the long-running host service lightweight (it only manages the loop and scope)
    /// while delegating business logic and transaction management to the scoped <see cref="TaskWorker"/>.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var taskWorker = scope.ServiceProvider.GetRequiredService<TaskWorker>();

            try
            {
                await taskWorker.ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Graceful shutdown requested - exit loop
                break;
            }
            catch (Exception exception)
            {
                // Log and continue - a single failing work item should not stop the background service
                Console.WriteLine(
                    $"Worker error: {exception.Message}");
            }
        }
    }
}
