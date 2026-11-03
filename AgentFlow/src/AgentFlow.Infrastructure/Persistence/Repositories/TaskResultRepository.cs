using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AgentFlow.Application.Abstractions.Persistence;

namespace AgentFlow.Infrastructure.Persistence.Repositories;

public sealed class TaskResultRepository : ITaskResultRepository
{
    private readonly AppDbContext _dbContext;

    public TaskResultRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TaskResult> AddAsync(
        TaskResult result,
        CancellationToken cancellationToken = default)
    {
        _dbContext.TaskResults.Add(result);

        return Task.FromResult(result);
    }

    public async Task<IReadOnlyList<TaskResult>> GetByTaskIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskResults
            .Where(result => result.TaskId == taskId)
            .OrderBy(result => result.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}