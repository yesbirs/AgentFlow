using AgentFlow.Application.Abstractions.Time;

namespace AgentFlow.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}