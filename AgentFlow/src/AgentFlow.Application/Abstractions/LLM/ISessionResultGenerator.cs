using AgentFlow.Application.Sessions.Services;

namespace AgentFlow.Application.Abstractions.LLM;

public interface ISessionResultGenerator
{
    Task<string> GenerateAsync(
        string sessionObjective,
        IReadOnlyList<SessionTaskOutput> taskOutputs,
        CancellationToken cancellationToken = default);
}