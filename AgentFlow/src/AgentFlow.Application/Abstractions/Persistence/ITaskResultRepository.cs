using AgentFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Abstractions.Persistence
{
    public interface ITaskResultRepository
    {
        Task<TaskResult> AddAsync(TaskResult taskResult, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TaskResult>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    }
}