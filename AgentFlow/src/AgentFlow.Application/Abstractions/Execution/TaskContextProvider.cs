using AgentFlow.Application.Abstractions.Execution;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Execution.Context;

public sealed class TaskContextProvider : ITaskContextProvider
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ITaskDependencyRepository _dependencyRepository;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskResultRepository _resultRepository;

    public TaskContextProvider(
        ISessionRepository sessionRepository,
        ITaskDependencyRepository dependencyRepository,
        IAgentTaskRepository taskRepository,
        ITaskResultRepository resultRepository)
    {
        _sessionRepository = sessionRepository;
        _dependencyRepository = dependencyRepository;
        _taskRepository = taskRepository;
        _resultRepository = resultRepository;
    }

    public async Task<TaskContext> BuildAsync(
        AgentTask task,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve the session
        var session = await _sessionRepository.GetSessionByIdAsync(
            task.SessionId,
            cancellationToken);

        if (session is null)
            throw new InvalidOperationException(
                $"Session {task.SessionId} was not found.");

        // 2. Retrieve dependency IDs
        var dependencyIds =
            await _dependencyRepository.GetDependencyTaskIdsAsync(
                task.Id,
                cancellationToken);

        var dependencyContexts = new List<DependencyContext>();

        // 3. Retrieve dependency task information and results
        foreach (var dependencyId in dependencyIds)
        {
            var dependencyTask =
                await _taskRepository.GetTaskByIdAsync(
                    dependencyId,
                    cancellationToken);

            if (dependencyTask is null)
                throw new InvalidOperationException(
                    $"Dependency task {dependencyId} was not found.");

            if (dependencyTask.State !=
                AgentFlow.Domain.Enums.TaskState.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Dependency task {dependencyId} has not succeeded.");
            }

            var results = await _resultRepository.GetByTaskIdAsync(
                dependencyId,
                cancellationToken);

            if (results.Count == 0)
                throw new InvalidOperationException(
                    $"Dependency task {dependencyId} has no results.");

            dependencyContexts.Add(
                new DependencyContext(
                    dependencyTask.Title,
                    dependencyTask.Description,
                    results[^1].Content));
        }

        // 4. Return complete execution context
        return new TaskContext(
            session.Prompt,
            task.Title,
            task.Description,
            dependencyContexts);
    }
}