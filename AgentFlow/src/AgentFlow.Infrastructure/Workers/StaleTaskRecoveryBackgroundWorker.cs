using AgentFlow.Application.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentFlow.API.BackgroundWorkers;

public sealed class StaleTaskRecoveryBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StaleTaskRecoveryBackgroundWorker> _logger;

    private static readonly TimeSpan RecoveryInterval =
        TimeSpan.FromSeconds(30);

    public StaleTaskRecoveryBackgroundWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<StaleTaskRecoveryBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Stale task recovery background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var recoveryService =
                    scope.ServiceProvider
                        .GetRequiredService<StaleTaskRecoveryService>();

                await recoveryService.RecoverAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while recovering stale tasks.");
            }

            try
            {
                await Task.Delay(
                    RecoveryInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Stale task recovery background worker stopped.");
    }
}