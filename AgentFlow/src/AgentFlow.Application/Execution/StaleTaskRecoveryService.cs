using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Time;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Application.Execution;

public sealed class StaleTaskRecoveryService
{
    private readonly IAgentTaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RetryPolicy _retryPolicy;

    private readonly TimeSpan _staleThreshold;
    private readonly IClock _clock;

    public StaleTaskRecoveryService(
        IAgentTaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        RetryPolicy retryPolicy,
        IClock clock)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
        _retryPolicy = retryPolicy;
        _clock = clock;

        _staleThreshold = TimeSpan.FromMinutes(10);
    }

    public async Task RecoverAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoff = _clock.UtcNow - _staleThreshold;

        var staleTasks =
            await _taskRepository.GetStaleRunningTasksAsync(
                cutoff,
                cancellationToken);

        foreach (var task in staleTasks)
        {
            task.RecoverFromStaleExecution();

            if (_retryPolicy.ShouldRetry(task))
            {
                task.Retry();
            }
        }

        if (staleTasks.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}