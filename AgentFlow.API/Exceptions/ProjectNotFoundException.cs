namespace AgentFlow.API.Exceptions
{
    public class ProjectNotFoundException : Exception
    {
        public ProjectNotFoundException(Guid id) : base($"Project with ID {id} not found.")
        {
        }
    }
}