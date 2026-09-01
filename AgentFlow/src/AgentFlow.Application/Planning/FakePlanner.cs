namespace AgentFlow.Application.Planning;

public sealed class FakePlanner : IPlanner
{
    public Task<PlanningResult> CreatePlanAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var tasks = new List<PlannedTask>
        {
            new(
                "research",
                "Research requirements",
                $"Analyze the requirements for: {prompt}"),

            new(
                "design",
                "Design solution",
                "Design the architecture and approach."),

            new(
                "implementation",
                "Implement solution",
                "Implement the designed solution."),

            new(
                "testing",
                "Test solution",
                "Verify that the implementation works correctly.")
        };

        var dependencies = new List<PlannedDependency>
        {
            new("design", "research"),
            new("implementation", "design"),
            new("testing", "implementation")
        };

        var result = new PlanningResult(tasks, dependencies);

        return Task.FromResult(result);
    }
}