using AgentFlow.API.Models;

namespace AgentFlow.API.Services
{
    public interface IWorkflowDefinitionService
    {
        Task<WorkFlowDefinition> CreateAsync(CreateWorkFlowDefinitionRequest request, CancellationToken cancellationToken);

        Task<WorkFlowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<WorkFlowDefinition> UpdateAsync(Guid id, UpdateWorkflowDefinitionRequest request, CancellationToken cancellationToken);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken);

        Task<PagedResult<WorkFlowDefinition>> GetFilteredAsync(WorkflowDefinitionQueryParameters queryParameters, CancellationToken cancellationToken);

        Task<WorkFlowDefinition> ActivateAsync(Guid id, CancellationToken cancellationToken);
    }
}
