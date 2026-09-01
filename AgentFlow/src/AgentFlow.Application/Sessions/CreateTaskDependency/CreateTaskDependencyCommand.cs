using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Sessions.CreateTaskDependency
{
    public sealed record CreateTaskDependencyCommand(Guid TaskId, Guid DependsOnTaskId);
}