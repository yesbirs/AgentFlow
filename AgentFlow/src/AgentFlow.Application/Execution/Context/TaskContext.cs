namespace AgentFlow.Application.Execution.Context;

public sealed record TaskContext(string SessionObjective, string TaskTitle, string TaskDescription, IReadOnlyList<DependencyContext> Dependencies);

public sealed record DependencyContext(string TaskTitle, string TaskDescription, string Result);