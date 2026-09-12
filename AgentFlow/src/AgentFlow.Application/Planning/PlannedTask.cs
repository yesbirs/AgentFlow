namespace AgentFlow.Application.Planning;

/// <summary>
/// Represents a planned task in the planner's output.
/// </summary>
public sealed record PlannedTask(string Key, string Title, string Description, string? ParentKey = null);
