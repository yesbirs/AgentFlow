using AgentFlow.API.Models;

namespace AgentFlow.API.Repositories
{
    public interface IWorkflowDefinitionRepository
    {
        Task<WorkFlowDefinition> CreateAsync(WorkFlowDefinition workflowDefinition, CancellationToken cancellationToken);

        Task<WorkFlowDefinition> UpdateAsync(WorkFlowDefinition workflowDefinition, CancellationToken cancellationToken);

        Task DeleteAsync(WorkFlowDefinition workflowDefinition, CancellationToken cancellationToken);

        Task<WorkFlowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<List<WorkFlowDefinition>> GetFilteredAsync(WorkflowDefinitionQueryParameters parameters, CancellationToken cancellationToken);

        Task<int> GetTotalCountAsync(WorkflowDefinitionQueryParameters parameters, CancellationToken cancellationToken);
    }
}
