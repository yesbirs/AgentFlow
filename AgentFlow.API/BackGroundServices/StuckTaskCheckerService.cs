using AgentFlow.API.Options;
using AgentFlow.API.Services;
using Microsoft.Extensions.Options;

namespace AgentFlow.API.BackGroundServices
{
    public class StuckTaskCheckerService : BackgroundService
    {
        private readonly ILogger<StuckTaskCheckerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly BackGroundServiceOptions _backGroundServiceOptions;
        private readonly PaginationOptions _paginationOptions;

        public StuckTaskCheckerService(ILogger<StuckTaskCheckerService> logger, IServiceProvider serviceProvider, IOptions<BackGroundServiceOptions> options, IOptions<PaginationOptions> paginationOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _backGroundServiceOptions = options.Value;
            _paginationOptions = paginationOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StuckTaskCheckerService has started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                        var batchSize = _backGroundServiceOptions.StuckTaskBatchSize;
                        if (batchSize < 1) batchSize = _paginationOptions.DefaultPageSize;
                        if (batchSize > _paginationOptions.MaxPageSize) batchSize = _paginationOptions.MaxPageSize;

                        var pendingTasks = await taskService.GetFilteredTasks(new Models.TaskQueryParameters() { Status = "pending", PageSize = batchSize }, stoppingToken);
                        _logger.LogInformation($"StuckTaskCheckerService has found {pendingTasks.Data.Count}");
                    }
                    await Task.Delay(TimeSpan.FromMinutes(_backGroundServiceOptions.StuckTaskCheckIntervalMinutes), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("StuckTaskCheckerService received cancellation signal");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for stuck tasks");
                    // Wait before retrying so we don't spam logs
                    await Task.Delay(TimeSpan.FromMinutes(_backGroundServiceOptions.StuckTaskCheckIntervalMinutes), stoppingToken);
                }
            }
            _logger.LogInformation("StuckTaskCheckerService is stopping.");
        }
    }
}