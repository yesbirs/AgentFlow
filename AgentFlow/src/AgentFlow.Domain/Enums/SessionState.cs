namespace AgentFlow.Domain.Enums;

public enum SessionState
{
    Created = 0,
    Planning = 1,
    Executing = 2,
    Finalizing = 3,
    Completed = 4,
    Failed = 5
}