using AgentFlow.API.Models;
using System.Threading;

namespace AgentFlow.API.Services
{
    public interface IProjectService
    {
        public Task<Project> CreateProject(CreateProjectRequest request, CancellationToken cancellationToken);

        public Task<Project?> UpdateProject(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken);

        public Task DeleteProject(Guid id, CancellationToken cancellationToken);

        public Task<Project?> GetProjectById(Guid id, CancellationToken cancellationToken);

        public Task<PagedResult<Project>> GetAllProjects(ProjectQueryParameters projectQueryParameters, CancellationToken cancellationToken);
    }
}