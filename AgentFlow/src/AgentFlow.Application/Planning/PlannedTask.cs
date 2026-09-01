namespace AgentFlow.Application.Planning;

public sealed record PlannedTask(string Key, string Title, string Description, string? ParentKey = null);