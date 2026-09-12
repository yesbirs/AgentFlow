using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle state of an <see cref="AgentFlow.Domain.Entities.AgentTask"/>.
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Task has been created and is waiting to be queued.
        /// </summary>
        Pending,

        /// <summary>
        /// Task has been queued for execution.
        /// </summary>
        Queued,

        /// <summary>
        /// Task is currently executing.
        /// </summary>
        Running,

        /// <summary>
        /// Task completed successfully.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Task failed during execution.
        /// </summary>
        Failed,

        /// <summary>
        /// Task was cancelled before completion.
        /// </summary>
        Cancelled
    }
}
