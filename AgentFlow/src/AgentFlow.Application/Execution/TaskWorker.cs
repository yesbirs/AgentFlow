using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Application.Execution;

/// <summary>
/// Worker responsible for receiving a single work item from the work queue and executing the corresponding task.
/// </summary>
public sealed class TaskWorker
{
    /// <summary>
    /// Queue used to receive work items.
    /// </summary>
    private readonly IWorkQueue _workQueue;

    /// <summary>
    /// Repository for agent tasks.
    /// </summary>
    private readonly IAgentTaskRepository _taskRepository;

    /// <summary>
    /// Component that executes task logic.
    /// </summary>
    private readonly ITaskExecutor _taskExecutor;

    /// <summary>
    /// Unit of work used to persist changes and manage transactions.
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Policy that decides whether failed tasks should be retried.
    /// </summary>
    private readonly RetryPolicy _retryPolicy;

    /// <summary>
    /// Initializes a new instance of <see cref="TaskWorker"/>.
    /// </summary>
    public TaskWorker(
        IWorkQueue workQueue,
        IAgentTaskRepository taskRepository,
        ITaskExecutor taskExecutor,
        IUnitOfWork unitOfWork,
        RetryPolicy retryPolicy)
    {
        _workQueue = workQueue;
        _taskRepository = taskRepository;
        _taskExecutor = taskExecutor;
        _unitOfWork = unitOfWork;
        _retryPolicy = retryPolicy;
    }

    /// <summary>
    /// Processes the next available work item from the queue. If no item is available, the method returns.
    /// </summary>
    public async Task ProcessNextAsync(
        CancellationToken cancellationToken = default)
    {
        var workItem = await _workQueue.ReceiveAsync(
            cancellationToken);

        if (workItem is null)
        {
            return;
        }

        var task = await _taskRepository.GetTaskByIdAsync(
            workItem.TaskId,
            cancellationToken);

        if (task is null)
        {
            await workItem.CompleteAsync(
                cancellationToken);

            return;
        }
        if (task.State != TaskState.Queued)
        {
            await workItem.CompleteAsync(
                cancellationToken);

            return;
        }

        task.Start();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        try
        {
            await _taskExecutor.ExecuteAsync(
                task,
                cancellationToken);

            task.Succeed();

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            await workItem.CompleteAsync(
                cancellationToken);
        }
        catch
        {
            task.Fail();

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            if (_retryPolicy.ShouldRetry(task))
            {
                task.Retry();

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await workItem.AbandonAsync(
                    cancellationToken);
            }
            else
            {
                await workItem.DeadLetterMessageAsync(
                    cancellationToken);
            }

            throw;
        }
    }
}