namespace AgentFlow.Application.Sessions.Services;

public sealed record SessionTaskOutput(
    Guid TaskId,
    string TaskTitle,
    string TaskDescription,
    string Content);