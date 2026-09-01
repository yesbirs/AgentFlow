namespace AgentFlow.Application.Planning;

public interface IPlanner
{
    Task<PlanningResult> CreatePlanAsync(string prompt, CancellationToken cancellationToken = default);
}