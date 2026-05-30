namespace AgentFlow.API.Exceptions
{
    public class WorkflowDefinitionNotFoundException : Exception
    {
        public WorkflowDefinitionNotFoundException(Guid workflowDefinitionId) : base($"Workflow definition with ID {workflowDefinitionId} not found.")
        {
        }
    }
}
