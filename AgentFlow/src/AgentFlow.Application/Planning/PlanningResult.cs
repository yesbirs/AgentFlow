namespace AgentFlow.Application.Planning;

/// <summary>
/// Result produced by a planner containing planned tasks and their dependencies.
/// </summary>
public sealed record PlanningResult(IReadOnlyList<PlannedTask> Tasks, IReadOnlyList<PlannedDependency> Dependencies);
