using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentFlow.Application.Sessions.CreateSession
{
    public sealed record CreateSessionCommand(string Prompt);
}