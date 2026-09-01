namespace AgentFlow.Application.Sessions.CreateTask
{
    public sealed record CreateTaskCommand(Guid SessionId, string Title, string Description, Guid? ParentTaskId = null);
}