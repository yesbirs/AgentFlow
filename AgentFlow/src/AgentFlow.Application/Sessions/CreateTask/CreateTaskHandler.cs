using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Sessions.CreateTask;

public sealed class CreateTaskHandler
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskHandler(
        ISessionRepository sessionRepository,
        IAgentTaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AgentTask> Handle(
    CreateTaskCommand command,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var sessionExists =
            await _sessionRepository.IsExistAsync(
                command.SessionId,
                cancellationToken);

        if (!sessionExists)
        {
            throw new InvalidOperationException(
                $"Session '{command.SessionId}' does not exist.");
        }

        if (command.ParentTaskId.HasValue)
        {
            var parentTask =
                await _taskRepository.GetTaskByIdAsync(
                    command.ParentTaskId.Value,
                    cancellationToken);

            if (parentTask is null)
            {
                throw new InvalidOperationException(
                    $"Parent task '{command.ParentTaskId}' does not exist.");
            }

            if (parentTask.SessionId != command.SessionId)
            {
                throw new InvalidOperationException(
                    "Parent task must belong to the same session.");
            }
        }

        var task = new AgentTask(
            command.SessionId,
            command.Title,
            command.Description,
            command.ParentTaskId);

        await _taskRepository.AddAsync(
            task,
            cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task;
    }
}