namespace AgentFlow.Application.Sessions.Queries;

public sealed record SessionTasksResponse(
    Guid SessionId,
    IReadOnlyList<SessionTaskDto> Tasks);

public sealed record SessionTaskDto(
    Guid TaskId,
    string Title,
    string Description,
    string State,
    DateTime CreatedAt,
    IReadOnlyList<TaskResultDto> Results);

public sealed record TaskResultDto(
    Guid Id,
    string Content,
    DateTime CreatedAt);