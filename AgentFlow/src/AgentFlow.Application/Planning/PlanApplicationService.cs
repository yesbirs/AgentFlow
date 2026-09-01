using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Planning;

public sealed class PlanApplicationService
{
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskDependencyRepository _dependencyRepository;

    public PlanApplicationService(
        IAgentTaskRepository taskRepository,
        ITaskDependencyRepository dependencyRepository)
    {
        _taskRepository = taskRepository;
        _dependencyRepository = dependencyRepository;
    }

    public async Task<IReadOnlyList<AgentTask>> ApplyAsync(
        Guid sessionId,
        PlanningResult plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var taskIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var createdTasks = new List<AgentTask>();

        foreach (var plannedTask in plan.Tasks)
        {
            var parentTaskId = ResolveParentTaskId(
                plannedTask,
                taskIds);

            var task = new AgentTask(
                sessionId,
                plannedTask.Title,
                plannedTask.Description,
                parentTaskId);

            await _taskRepository.AddAsync(
                task,
                cancellationToken);

            taskIds.Add(
                plannedTask.Key,
                task.Id);

            createdTasks.Add(task);
        }

        foreach (var plannedDependency in plan.Dependencies)
        {
            var taskId = taskIds[
                plannedDependency.TaskKey];

            var dependsOnTaskId = taskIds[
                plannedDependency.DependsOnTaskKey];

            var dependency = new TaskDependency(
                taskId,
                dependsOnTaskId);

            await _dependencyRepository.AddAsync(
                dependency,
                cancellationToken);
        }

        return createdTasks;
    }

    private static Guid? ResolveParentTaskId(
        PlannedTask plannedTask,
        Dictionary<string, Guid> taskIds)
    {
        if (plannedTask.ParentKey is null)
        {
            return null;
        }

        if (!taskIds.TryGetValue(plannedTask.ParentKey, out var parentId))
        {
            throw new InvalidOperationException(
                $"Parent task '{plannedTask.ParentKey}' " +
                $"must appear before '{plannedTask.Key}'.");
        }

        return parentId;
    }
}