using AgentFlow.API.Exceptions;
using AgentFlow.API.Models;
using AgentFlow.API.Repositories;

namespace AgentFlow.API.Services
{
    public class PostgreSqlWorkflowDefinitionService : IWorkflowDefinitionService
    {
        private readonly ILogger<PostgreSqlWorkflowDefinitionService> _logger;
        private readonly IWorkflowDefinitionRepository _repository;

        public PostgreSqlWorkflowDefinitionService(IWorkflowDefinitionRepository repository, ILogger<PostgreSqlWorkflowDefinitionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<WorkFlowDefinition> CreateAsync(CreateWorkFlowDefinitionRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating workflow definition: {WorkflowName}", request.Name);

            var workflowDefinition = new WorkFlowDefinition
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StepsJson = request.StepsJson ?? "[]",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Version = 1
            };

            await _repository.CreateAsync(workflowDefinition, cancellationToken);

            _logger.LogInformation("Workflow definition created with ID: {WorkflowId}", workflowDefinition.Id);
            return workflowDefinition;
        }

        public async Task<WorkFlowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching workflow definition: {WorkflowId}", id);
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<WorkFlowDefinition> UpdateAsync(Guid id, UpdateWorkflowDefinitionRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating workflow definition: {WorkflowId}", id);

            var workflowDefinition = await _repository.GetByIdAsync(id, cancellationToken);

            if (workflowDefinition == null)
            {
                _logger.LogWarning("Workflow definition {WorkflowId} not found", id);
                throw new WorkflowDefinitionNotFoundException(id);
            }

            workflowDefinition.Name = request.Name;
            workflowDefinition.Description = request.Description;
            workflowDefinition.StepsJson = request.StepsJson ?? workflowDefinition.StepsJson;
            workflowDefinition.IsActive = request.IsActive;
            workflowDefinition.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(workflowDefinition, cancellationToken);

            _logger.LogInformation("Workflow definition {WorkflowId} updated", id);
            return workflowDefinition;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Soft deleting workflow definition: {WorkflowId}", id);

            var workflowDefinition = await _repository.GetByIdAsync(id, cancellationToken);

            if (workflowDefinition == null)
            {
                _logger.LogWarning("Workflow definition {WorkflowId} not found", id);
                throw new WorkflowDefinitionNotFoundException(id);
            }

            workflowDefinition.IsActive = false;
            workflowDefinition.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(workflowDefinition, cancellationToken);

            _logger.LogInformation("Workflow definition {WorkflowId} soft deleted", id);
        }

        public async Task<PagedResult<WorkFlowDefinition>> GetFilteredAsync(WorkflowDefinitionQueryParameters queryParameters, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching filtered workflow definitions: page {PageNumber}, size {PageSize}, isActive {IsActive}",
                queryParameters.PageNumber,
                queryParameters.PageSize,
                queryParameters.IsActive);

            var workflowDefinitions = await _repository.GetFilteredAsync(queryParameters, cancellationToken);
            var totalCount = await _repository.GetTotalCountAsync(queryParameters.IsActive, cancellationToken);

            return new PagedResult<WorkFlowDefinition>
            {
                Data = workflowDefinitions,
                TotalCount = totalCount,
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize
            };
        }

        public async Task<WorkFlowDefinition> ActivateAsync(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating workflow definition: {WorkflowId}", id);

            var workflowDefinition = await _repository.GetByIdAsync(id, cancellationToken);

            if (workflowDefinition == null)
            {
                _logger.LogWarning("Workflow definition {WorkflowId} not found", id);
                throw new WorkflowDefinitionNotFoundException(id);
            }

            workflowDefinition.IsActive = true;
            workflowDefinition.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(workflowDefinition, cancellationToken);

            _logger.LogInformation("Workflow definition {WorkflowId} activated", id);
            return workflowDefinition;
        }
    }
}
