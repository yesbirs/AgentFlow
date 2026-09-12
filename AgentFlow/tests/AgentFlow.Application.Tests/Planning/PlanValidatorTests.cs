using AgentFlow.Application.Planning;

namespace AgentFlow.Application.Tests.Planning;

/// <summary>
/// Unit tests for <see cref="AgentFlow.Application.Planning.PlanValidator"/> ensuring plans are structurally valid.
/// </summary>
public class PlanValidatorTests
{
    [Fact]
    public void Accepts_valid_plan()
    {
        var plan = new PlanningResult(
            new[]
            {
                new PlannedTask(
                    "research",
                    "Research",
                    "Research requirements."),

                new PlannedTask(
                    "implementation",
                    "Implement",
                    "Implement solution.")
            },
            new[]
            {
                new PlannedDependency(
                    "implementation",
                    "research")
            });

        var validator = new PlanValidator();

        validator.Validate(plan);
    }

    [Fact]
    public void Rejects_duplicate_task_keys()
    {
        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "research",
                "Research",
                "First"),

            new PlannedTask(
                "research",
                "Research again",
                "Second")
            },
            Array.Empty<PlannedDependency>());

        var validator = new PlanValidator();

        Assert.Throws<InvalidOperationException>(
            () => validator.Validate(plan));
    }

    [Fact]
    public void Rejects_missing_dependency_task()
    {
        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "implementation",
                "Implement",
                "Implement solution.")
            },
            new[]
            {
            new PlannedDependency(
                "implementation",
                "research")
            });

        var validator = new PlanValidator();

        Assert.Throws<InvalidOperationException>(
            () => validator.Validate(plan));
    }

    [Fact]
    public void Rejects_dependency_cycle()
    {
        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "A",
                "Task A",
                "A"),

            new PlannedTask(
                "B",
                "Task B",
                "B"),

            new PlannedTask(
                "C",
                "Task C",
                "C")
            },
            new[]
            {
            new PlannedDependency("A", "B"),
            new PlannedDependency("B", "C"),
            new PlannedDependency("C", "A")
            });

        var validator = new PlanValidator();

        Assert.Throws<InvalidOperationException>(
            () => validator.Validate(plan));
    }

    [Fact]
    public void Rejects_self_dependency()
    {
        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "A",
                "Task A",
                "A")
            },
            new[]
            {
            new PlannedDependency("A", "A")
            });

        var validator = new PlanValidator();

        Assert.Throws<InvalidOperationException>(
            () => validator.Validate(plan));
    }
}