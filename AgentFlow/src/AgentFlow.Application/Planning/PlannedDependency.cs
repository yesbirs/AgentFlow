namespace AgentFlow.Application.Planning;

/// <summary>
/// Represents a dependency between two planned tasks.
/// </summary>
public sealed record PlannedDependency(string TaskKey, string DependsOnTaskKey);
