using AgentFlow.API.Data;
using AgentFlow.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace AgentFlow.API.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private AgentFlowDBContext _context;

        public ProjectRepository(AgentFlowDBContext context)
        {
            _context = context;
        }

        public async Task DeleteProject(Project project, CancellationToken cancellationToken)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Project>> GetAllProjects(ProjectQueryParameters projectQueryParameters, CancellationToken cancellationToken)
        {
            var query = _context.Projects.Include(p => p.Tasks).AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(projectQueryParameters.Status))
            {
                query = query.Where(p => p.Status == projectQueryParameters.Status);
            }
            if (!string.IsNullOrEmpty(projectQueryParameters.OwnerEmail))
            {
                query = query.Where(p => p.OwnerEmail == projectQueryParameters.OwnerEmail);
            }

            if (!string.IsNullOrEmpty(projectQueryParameters.CreatedByUserId))
            {
                query = query.Where(p => p.CreatedByUserId == projectQueryParameters.CreatedByUserId);
            }

            // Sorting
            query = projectQueryParameters.SortBy?.ToLower() switch
            {
                "name" => projectQueryParameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.Name),

                _ => projectQueryParameters.SortOrder?.ToLower() == "asc"
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt)
            };

            // Pagination
            query = query
                .Skip((projectQueryParameters.PageNumber - 1) * projectQueryParameters.PageSize)
                .Take(projectQueryParameters.PageSize);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken)
        {
            return await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        }

        public async Task<Project> UpdateProject(Project project, CancellationToken cancellationToken)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync(cancellationToken);
            return project;
        }

        public async Task<Project> CreateProject(Project project, CancellationToken cancellationToken)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);
            return project;
        }

        public async Task<Project?> GetProjectByIdWithTasks(Guid projectId, CancellationToken cancellationToken)
        {
            return await _context.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        }

        public async Task<int> GetTotalProjectCounts(ProjectQueryParameters projectQueryParameters, CancellationToken cancellationToken)
        {
            var query = _context.Projects.AsQueryable();

            if (!string.IsNullOrEmpty(projectQueryParameters.Status))
            {
                query = query.Where(p => p.Status == projectQueryParameters.Status);
            }

            if (!string.IsNullOrEmpty(projectQueryParameters.OwnerEmail))
            {
                query = query.Where(p => p.OwnerEmail == projectQueryParameters.OwnerEmail);
            }

            if (!string.IsNullOrEmpty(projectQueryParameters.CreatedByUserId))
            {
                query = query.Where(p => p.CreatedByUserId == projectQueryParameters.CreatedByUserId);
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}