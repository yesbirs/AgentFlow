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
    /// <summary>
    /// Repository implementation for storing and retrieving task dependency entities.
    /// </summary>
    public class TaskDependencyRepository : ITaskDependencyRepository
    {
        private readonly AppDbContext _context;

        public TaskDependencyRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a new task dependency to the context.
        /// </summary>
        public async Task AddAsync(TaskDependency taskDependency, CancellationToken cancellationToken)
        {
            await _context.TaskDependencies.AddAsync(taskDependency, cancellationToken);
        }

        /// <summary>
        /// Retrieves the ids of tasks that the specified task depends on.
        /// </summary>
        public async Task<IReadOnlyList<Guid>> GetDependencyTaskIdsAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return await _context.TaskDependencies
                .Where(dependency => dependency.TaskId == taskId)
                .Select(dependency => dependency.DependsOnTaskId)
                .ToListAsync(cancellationToken);
        }
    }
}