using AgentFlow.API.Models;
using System.Threading;

namespace AgentFlow.API.Services
{
    public interface ITaskService
    {
        Task<AgentTask> CreateTask(CreateTaskRequest request, string? userId, CancellationToken cancellationToken);

        Task<AgentTask?> GetTaskById(Guid id, string? userId, bool isAdmin, CancellationToken cancellationToken);

        Task<AgentTask> UpdateTask(Guid id, UpdateTaskRequest updatedTask, string? userId, bool isAdmin, CancellationToken cancellationToken);

        Task DeleteTask(Guid id, CancellationToken cancellationToken);

        Task<PagedResult<AgentTask>> GetFilteredTasks(TaskQueryParameters queryParameters, CancellationToken cancellationToken);

        Task<int> GetTotalTasksCount(CancellationToken cancellationToken);
    }
}