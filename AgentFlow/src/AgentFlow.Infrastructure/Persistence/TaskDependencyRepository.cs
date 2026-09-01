using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
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
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}