using AgentFlow.API.Options;
using Microsoft.Extensions.Options;

namespace AgentFlow.API.BackGroundServices
{
    public class HeartbeatBackgroundService : BackgroundService
    {
        private readonly ILogger<HeartbeatBackgroundService> _logger;
        private readonly BackGroundServiceOptions _backGroundServiceOptions;

        public HeartbeatBackgroundService(ILogger<HeartbeatBackgroundService> logger, IOptions<BackGroundServiceOptions> options)
        {
            _logger = logger;
            _backGroundServiceOptions = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HeartbeatBackgroundService is started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Heartbeat: AgentFlow API is alive at {time}", DateTime.Now);
                try
                {
                    // You can add additional health checks or diagnostics here if needed
                    await Task.Delay(TimeSpan.FromSeconds(_backGroundServiceOptions.HeartbeatIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Heartbeat service received cancellation signal");
                    break;
                }
            }
            _logger.LogInformation("Heartbeat service stopped gracefully");
        }
    }
}