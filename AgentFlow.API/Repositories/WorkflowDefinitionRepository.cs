using AgentFlow.API.Data;
using AgentFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.API.Repositories
{
    public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly AgentFlowDBContext _dbContext;

        public WorkflowDefinitionRepository(AgentFlowDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkFlowDefinition> CreateAsync(WorkFlowDefinition workflowDefinition, CancellationToken cancellationToken)
        {
            _dbContext.WorkflowDefinitions.Add(workflowDefinition);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return workflowDefinition;
        }

        public async Task<WorkFlowDefinition> UpdateAsync(WorkFlowDefinition workflowDefinition, CancellationToken cancellationToken)
        {
            _dbContext.WorkflowDefinitions.Update(workflowDefinition);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return workflowDefinition;
        }

        public async Task DeleteAsync(WorkFlowDefinition workflowDefinition, CancellationToken cancellationToken)
        {
            _dbContext.WorkflowDefinitions.Remove(workflowDefinition);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<WorkFlowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<List<WorkFlowDefinition>> GetFilteredAsync(WorkflowDefinitionQueryParameters parameters, CancellationToken cancellationToken)
        {
            var query = _dbContext.WorkflowDefinitions.AsQueryable();

            // Filtering
            if (parameters.IsActive.HasValue)
            {
                query = query.Where(w => w.IsActive == parameters.IsActive.Value);
            }

            // Sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "name" => parameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(w => w.Name)
                    : query.OrderByDescending(w => w.Name),

                "createdat" => parameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(w => w.CreatedAt)
                    : query.OrderByDescending(w => w.CreatedAt),

                _ => parameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(w => w.CreatedAt)
                    : query.OrderByDescending(w => w.CreatedAt)
            };

            // Pagination
            query = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalCountAsync(bool? isActive, CancellationToken cancellationToken)
        {
            var query = _dbContext.WorkflowDefinitions.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(w => w.IsActive == isActive.Value);
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}
