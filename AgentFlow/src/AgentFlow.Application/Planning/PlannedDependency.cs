namespace AgentFlow.Application.Planning;

public sealed record PlannedDependency(string TaskKey, string DependsOnTaskKey);