using AgentFlow.Application.Abstractions.Time;

namespace AgentFlow.Application.Tests;

public sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; set; }
}
