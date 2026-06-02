using AgentFlow.API.Data;
using AgentFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AgentFlow.API.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AgentFlowDBContext _dbContext;

        public TaskRepository(AgentFlowDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AgentTask> CreateTask(AgentTask task, CancellationToken cancellationToken)
        {
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task DeleteTask(AgentTask task, CancellationToken cancellationToken)
        {
            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<AgentTask?> GetTask(Guid taskId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        }

        public async Task<AgentTask> UpdateTask(AgentTask task, CancellationToken cancellationToken)
        {
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return task;
        }

        public async Task<List<AgentTask>> GetTasksByProjectId(Guid projectId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks.Where(t => t.ProjectId == projectId).ToListAsync(cancellationToken);
        }

        public async Task<List<AgentTask>> GetTasksByStatus(string status, CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks.Where(t => t.Status == status).ToListAsync(cancellationToken);
        }

        public async Task<List<AgentTask>> GetTaskByIdWithProject(Guid taskId, CancellationToken cancellationToken)
        {
            return await _dbContext.Tasks.Include(t => t.Project).Where(t => t.Id == taskId).ToListAsync(cancellationToken);
        }

        // Filtered, sorted, paginated query
        public async Task<List<AgentTask>> GetFilteredAsync(TaskQueryParameters parameters, CancellationToken cancellationToken)
        {
            // Start with IQueryable - the query is NOT executed yet
            var query = _dbContext.Tasks
                .Include(t => t.Project)
                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(t => t.Status == parameters.Status);
            }

            if (!string.IsNullOrEmpty(parameters.Priority))
            {
                query = query.Where(t => t.Priority == parameters.Priority);
            }

            if (parameters.ProjectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == parameters.ProjectId);
            }

            if (!string.IsNullOrEmpty(parameters.CreatedByUserId))
            {
                query = query.Where(t => t.CreatedByUserId == parameters.CreatedByUserId);
            }

            // Sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "name" => parameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Name)
                    : query.OrderByDescending(t => t.Name),

                "priority" => parameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(t => t.Priority)
                    : query.OrderByDescending(t => t.Priority),

                _ => parameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(t => t.CreatedAt)
                    : query.OrderByDescending(t => t.CreatedAt)
            };

            // Pagination
            query = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalTasksCount(TaskQueryParameters parameters, CancellationToken cancellationToken)
        {
            var query = _dbContext.Tasks.AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(t => t.Status == parameters.Status);
            }

            if (!string.IsNullOrEmpty(parameters.Priority))
            {
                query = query.Where(t => t.Priority == parameters.Priority);
            }

            if (parameters.ProjectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == parameters.ProjectId);
            }

            if (!string.IsNullOrEmpty(parameters.CreatedByUserId))
            {
                query = query.Where(t => t.CreatedByUserId == parameters.CreatedByUserId);
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}