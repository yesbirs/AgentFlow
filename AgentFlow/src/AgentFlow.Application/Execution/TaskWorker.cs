using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;

namespace AgentFlow.Application.Execution;

public sealed class TaskWorker
{
    private readonly IWorkQueue _workQueue;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskExecutor _taskExecutor;

    public TaskWorker(
        IWorkQueue workQueue,
        IAgentTaskRepository taskRepository,
        ITaskExecutor taskExecutor)
    {
        _workQueue = workQueue;
        _taskRepository = taskRepository;
        _taskExecutor = taskExecutor;
    }

    public async Task ProcessNextAsync(
        CancellationToken cancellationToken = default)
    {
        var taskId = await _workQueue.DequeueAsync(
            cancellationToken);

        var task = await _taskRepository.GetTaskByIdAsync(
            taskId,
            cancellationToken);

        if (task is null)
        {
            return;
        }

        task.Start();

        await _taskExecutor.ExecuteAsync(
            task,
            cancellationToken);

        task.Succeed();
    }
}