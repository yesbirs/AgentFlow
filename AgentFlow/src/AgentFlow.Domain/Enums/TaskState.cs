using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Domain.Enums
{
    public enum TaskState
    {
        Pending,
        Queued,
        Running,
        Succeeded,
        Failed,
        Cancelled
    }
}