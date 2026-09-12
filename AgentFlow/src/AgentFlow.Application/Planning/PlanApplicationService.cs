using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Planning;

/// <summary>
/// Service responsible for applying a planning result to the data model by creating tasks and dependencies.
/// </summary>
public sealed class PlanApplicationService
{
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskDependencyRepository _dependencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of <see cref="PlanApplicationService"/>.
    /// </summary>
    public PlanApplicationService(
        IAgentTaskRepository taskRepository,
        ITaskDependencyRepository dependencyRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _dependencyRepository = dependencyRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Creates tasks and dependencies from the provided planning result for the given session.
    /// Returns the list of created <see cref="AgentTask"/> instances.
    /// </summary>
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return createdTasks;
    }

    /// <summary>
    /// Resolves a planned task's parent id using the map of created task ids.
    /// Throws when the parent key is missing in the provided map.
    /// </summary>
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
