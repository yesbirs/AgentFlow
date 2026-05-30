using AgentFlow.API.Models;
using System.Threading;

namespace AgentFlow.API.Repositories
{
    public interface IProjectRepository
    {
        public Task<Project> CreateProject(Project project, CancellationToken cancellationToken);

        public Task<Project> UpdateProject(Project project, CancellationToken cancellationToken);

        public Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken);

        public Task DeleteProject(Project project, CancellationToken cancellationToken);

        public Task<List<Project>> GetAllProjects(ProjectQueryParameters projectQueryParameters, CancellationToken cancellationToken);

        public Task<Project?> GetProjectByIdWithTasks(Guid projectId, CancellationToken cancellationToken);

        public Task<int> GetTotalProjectCounts(CancellationToken cancellationToken);
    }
}