namespace AgentFlow.Application.Planning;

public sealed record PlanningResult(IReadOnlyList<PlannedTask> Tasks, IReadOnlyList<PlannedDependency> Dependencies);