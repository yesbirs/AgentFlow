using AgentFlow.Application.Abstractions.Persistence;

namespace AgentFlow.Application.Sessions.Queries;

public sealed class GetSessionTasksHandler
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskResultRepository _taskResultRepository;

    public GetSessionTasksHandler(
        ISessionRepository sessionRepository,
        IAgentTaskRepository taskRepository,
        ITaskResultRepository taskResultRepository)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _taskResultRepository = taskResultRepository;
    }

    public async Task<SessionTasksResponse?> HandleAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        // 1. Verify that the session exists.
        var session = await _sessionRepository.GetSessionByIdAsync(
            sessionId,
            cancellationToken);

        if (session is null)
            return null;

        // 2. Retrieve all tasks belonging to this session.
        var tasks = await _taskRepository.GetBySessionIdAsync(
            sessionId,
            cancellationToken);

        // 3. Retrieve results for each task.
        var taskDtos = new List<SessionTaskDto>();

        foreach (var task in tasks)
        {
            var results = await _taskResultRepository.GetByTaskIdAsync(
                task.Id,
                cancellationToken);

            var resultDtos = results
                .Select(result => new TaskResultDto(
                    result.Id,
                    result.Content,
                    result.CreatedAt))
                .ToList();

            taskDtos.Add(new SessionTaskDto(
                task.Id,
                task.Title,
                task.Description,
                task.State.ToString(),
                task.CreatedAt,
                resultDtos));
        }

        // 4. Return the complete session task response.
        return new SessionTasksResponse(
            sessionId,
            taskDtos);
    }
}