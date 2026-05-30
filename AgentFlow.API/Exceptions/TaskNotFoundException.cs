namespace AgentFlow.API.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException(Guid taskId) : base($"Task with ID {taskId} not found.")
        {
        }
    }
}