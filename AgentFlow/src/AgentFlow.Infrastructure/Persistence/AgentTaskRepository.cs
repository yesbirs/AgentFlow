using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Infrastructure.Persistence
{
    public class AgentTaskRepository : IAgentTaskRepository
    {
        private readonly AppDbContext _context;

        public AgentTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AgentTask task, CancellationToken cancellationToken = default)
        {
            await _context.Tasks.AddAsync(task, cancellationToken);
        }

        public async Task<AgentTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken)
        {
            return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        }

        public async Task<IReadOnlyList<AgentTask>> GetPendingTasksAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.Where(task => task.SessionId == sessionId && task.State == TaskState.Pending).ToListAsync(cancellationToken);
        }

        public async Task<bool> TryClaimAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(
                    x => x.Id == taskId,
                    cancellationToken);

            if (task is null)
            {
                return false;
            }

            if (task.State != TaskState.Pending)
            {
                return false;
            }

            task.Queue();

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}