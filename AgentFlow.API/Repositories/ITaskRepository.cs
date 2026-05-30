using AgentFlow.API.Models;
using System.Threading;

namespace AgentFlow.API.Repositories
{
    public interface ITaskRepository
    {
        public Task<AgentTask> CreateTask(AgentTask task, CancellationToken cancellationToken);

        public Task<AgentTask> UpdateTask(AgentTask task, CancellationToken cancellationToken);

        public Task DeleteTask(AgentTask task, CancellationToken cancellationToken);

        public Task<AgentTask?> GetTask(Guid taskId, CancellationToken cancellationToken);

        public Task<List<AgentTask>> GetTasksByProjectId(Guid projectId, CancellationToken cancellationToken);

        public Task<List<AgentTask>> GetTasksByStatus(string status, CancellationToken cancellationToken);

        public Task<List<AgentTask>> GetTaskByIdWithProject(Guid taskId, CancellationToken cancellationToken);

        public Task<List<AgentTask>> GetFilteredAsync(TaskQueryParameters parameters, CancellationToken cancellationToken);

        public Task<int> GetTotalTasksCount(CancellationToken cancellationToken);
    }
}