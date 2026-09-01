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
    public class AgentTaskRepository : IAgentTaskRepository
    {
        private readonly AppDbContext _context;

        public AgentTaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AgentTask task, CancellationToken cancellationToken = default)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<AgentTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken)
        {
            return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        }
    }
}