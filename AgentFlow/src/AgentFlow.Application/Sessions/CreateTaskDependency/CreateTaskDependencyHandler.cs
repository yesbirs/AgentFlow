using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Sessions.CreateTaskDependency;

public sealed class CreateTaskDependencyHandler
{
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskDependencyRepository _dependencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskDependencyHandler(
        IAgentTaskRepository taskRepository,
        ITaskDependencyRepository dependencyRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _dependencyRepository = dependencyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaskDependency> Handle(
        CreateTaskDependencyCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = await _taskRepository.GetTaskByIdAsync(
            command.TaskId,
            cancellationToken);

        if (task is null)
        {
            throw new InvalidOperationException(
                $"Task '{command.TaskId}' does not exist.");
        }

        var dependencyTask =
            await _taskRepository.GetTaskByIdAsync(
                command.DependsOnTaskId,
                cancellationToken);

        if (dependencyTask is null)
        {
            throw new InvalidOperationException(
                $"Dependency task '{command.DependsOnTaskId}' does not exist.");
        }

        if (task.SessionId != dependencyTask.SessionId)
        {
            throw new InvalidOperationException(
                "Tasks must belong to the same session.");
        }

        var dependency = new TaskDependency(
            command.TaskId,
            command.DependsOnTaskId);

        await _dependencyRepository.AddAsync(
            dependency,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return dependency;
    }
}