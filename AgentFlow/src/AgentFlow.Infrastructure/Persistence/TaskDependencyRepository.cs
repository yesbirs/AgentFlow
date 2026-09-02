using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Infrastructure.Persistence
{
    public class TaskDependencyRepository : ITaskDependencyRepository
    {
        private readonly AppDbContext _context;

        public TaskDependencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TaskDependency taskDependency, CancellationToken cancellationToken)
        {
            await _context.TaskDependencies.AddAsync(taskDependency, cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetDependencyTaskIdsAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return await _context.TaskDependencies
                .Where(dependency => dependency.TaskId == taskId)
                .Select(dependency => dependency.DependsOnTaskId)
                .ToListAsync(cancellationToken);
        }
    }
}