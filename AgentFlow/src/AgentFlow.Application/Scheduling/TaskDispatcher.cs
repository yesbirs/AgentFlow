using System.Text.Json;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Scheduling;

public sealed class TaskDispatcher
{
    private readonly IScheduler _scheduler;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TaskDispatcher(
        IScheduler scheduler,
        IAgentTaskRepository taskRepository,
        IOutboxMessageRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduler = scheduler;
        _taskRepository = taskRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> DispatchReadyTasksAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var readyTasksForQueue = await _scheduler.FindReadyTasksAsync(
            sessionId,
            cancellationToken);

        var dispatchedCount = 0;

        foreach (var task in readyTasksForQueue)
        {
            await _unitOfWork.BeginTransactionAsync(
                cancellationToken);

            try
            {
                var claimed = await _taskRepository.TryClaimAsync(
                    task.Id,
                    cancellationToken);

                if (!claimed)
                {
                    await _unitOfWork.RollbackTransactionAsync(
                        cancellationToken);

                    continue;
                }

                var payload = JsonSerializer.Serialize(new ExecuteTaskMessage(task.Id));

                var outboxMessage = new OutboxMessage(
                    "ExecuteTask",
                    payload);

                await _outboxRepository.AddAsync(
                    outboxMessage,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                await _unitOfWork.CommitTransactionAsync(
                    cancellationToken);

                dispatchedCount++;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(
                    cancellationToken);

                throw;
            }
        }

        return dispatchedCount;
    }
}