using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AgentFlow.Infrastructure.Workers;

public sealed class SchedulerBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SchedulerBackgroundWorker(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var sessionRepository = scope.ServiceProvider
                        .GetRequiredService<ISessionRepository>();

                var dispatcher = scope.ServiceProvider
                        .GetRequiredService<TaskDispatcher>();

                var sessionIds = await sessionRepository
                        .GetSessionIdsWithPendingTasksAsync(
                            stoppingToken);

                foreach (var sessionId in sessionIds)
                {
                    await dispatcher.DispatchReadyTasksAsync(
                        sessionId,
                        stoppingToken);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Scheduler error: {exception.Message}");
            }
        }
    }
}