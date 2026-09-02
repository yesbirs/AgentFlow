using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Application.Scheduling;

public sealed class Scheduler : IScheduler
{
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskDependencyRepository _dependencyRepository;

    public Scheduler(
        IAgentTaskRepository taskRepository,
        ITaskDependencyRepository dependencyRepository)
    {
        _taskRepository = taskRepository;
        _dependencyRepository = dependencyRepository;
    }

    public async Task<IReadOnlyList<AgentTask>> FindReadyTasksAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var pendingTasks = await _taskRepository.GetPendingTasksAsync(
            sessionId,
            cancellationToken);

        var readyTasks = new List<AgentTask>();

        foreach (var task in pendingTasks)
        {
            var dependencyIds =
                await _dependencyRepository.GetDependencyTaskIdsAsync(
                    task.Id,
                    cancellationToken);

            if (dependencyIds.Count == 0)
            {
                readyTasks.Add(task);
                continue;
            }

            var allDependenciesSucceeded = true;

            foreach (var dependencyId in dependencyIds)
            {
                var dependency = await _taskRepository.GetTaskByIdAsync(
                    dependencyId,
                    cancellationToken);

                if (dependency is null ||
                    dependency.State != TaskState.Succeeded)
                {
                    allDependenciesSucceeded = false;
                    break;
                }
            }

            if (allDependenciesSucceeded)
            {
                readyTasks.Add(task);
            }
        }

        return readyTasks;
    }
}