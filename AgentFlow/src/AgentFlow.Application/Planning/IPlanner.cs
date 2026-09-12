namespace AgentFlow.Application.Planning;

/// <summary>
/// Planner abstraction that produces a plan from a text prompt.
/// </summary>
public interface IPlanner
{
    /// <summary>
    /// Creates a planning result based on the provided prompt.
    /// </summary>
    Task<PlanningResult> CreatePlanAsync(string prompt, CancellationToken cancellationToken = default);
}
