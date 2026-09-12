using AgentFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Abstractions.Persistence
{
    /// <summary>
    /// Repository contract for persisting task dependency relationships.
    /// </summary>
    public interface ITaskDependencyRepository
    {
        /// <summary>
        /// Adds a task dependency relationship.
        /// </summary>
        Task AddAsync(TaskDependency taskDependency, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the ids of tasks that the specified task depends on.
        /// </summary>
        Task<IReadOnlyList<Guid>> GetDependencyTaskIdsAsync(Guid taskId, CancellationToken cancellationToken = default);
    }
}
