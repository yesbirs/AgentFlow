using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;

namespace AgentFlow.Application.Scheduling;

public sealed class TaskDispatcher
{
    private readonly IScheduler _scheduler;
    private readonly IWorkQueue _workQueue;
    private readonly IAgentTaskRepository _taskRepository;

    public TaskDispatcher(
     IScheduler scheduler,
     IWorkQueue workQueue,
     IAgentTaskRepository taskRepository)
    {
        _scheduler = scheduler;
        _workQueue = workQueue;
        _taskRepository = taskRepository;
    }

    public async Task<int> DispatchReadyTasksAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var readyTasks = await _scheduler.FindReadyTasksAsync(
            sessionId,
            cancellationToken);

        var dispatchedCount = 0;

        foreach (var task in readyTasks)
        {
            var claimed = await _taskRepository.TryClaimAsync(
                task.Id,
                cancellationToken);

            if (!claimed)
            {
                continue;
            }

            await _workQueue.EnqueueAsync(
                task.Id,
                cancellationToken);

            dispatchedCount++;
        }

        return dispatchedCount;
    }
}