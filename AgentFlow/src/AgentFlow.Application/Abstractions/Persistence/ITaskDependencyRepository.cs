using AgentFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Abstractions.Persistence
{
    public interface ITaskDependencyRepository
    {
        Task AddAsync(TaskDependency taskDependency, CancellationToken cancellationToken = default);
    }
}