using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Planning;
using AgentFlow.Domain.Entities;
using Moq;

namespace AgentFlow.Application.Tests.Planning;

public class PlanApplicationServiceTests
{
    [Fact]
    public async Task Applies_plan_and_creates_all_tasks()
    {
        var sessionId = Guid.NewGuid();

        var taskRepository =
            new Mock<IAgentTaskRepository>();

        var dependencyRepository =
            new Mock<ITaskDependencyRepository>();

        var plan = new PlanningResult(
            new[]
            {
                new PlannedTask(
                    "research",
                    "Research",
                    "Research requirements."),

                new PlannedTask(
                    "implementation",
                    "Implementation",
                    "Implement the solution.")
            },
            Array.Empty<PlannedDependency>());

        var service = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await service.ApplyAsync(
            sessionId,
            plan);

        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            task => Assert.Equal(
                sessionId,
                task.SessionId));

        taskRepository.Verify(
            x => x.AddAsync(
                It.IsAny<AgentTask>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Applies_plan_and_creates_dependencies()
    {
        var sessionId = Guid.NewGuid();

        var taskRepository =
            new Mock<IAgentTaskRepository>();

        var dependencyRepository =
            new Mock<ITaskDependencyRepository>();

        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "research",
                "Research",
                "Research requirements."),

            new PlannedTask(
                "implementation",
                "Implementation",
                "Implement the solution.")
            },
            new[]
            {
            new PlannedDependency(
                "implementation",
                "research")
            });

        var service = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await service.ApplyAsync(
            sessionId,
            plan);

        Assert.Equal(2, result.Count);

        var research = result.Single(
            x => x.Title == "Research");

        var implementation = result.Single(
            x => x.Title == "Implementation");

        dependencyRepository.Verify(
            x => x.AddAsync(
                It.Is<TaskDependency>(
                    d =>
                        d.TaskId == implementation.Id &&
                        d.DependsOnTaskId == research.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Resolves_parent_task_key_to_parent_guid()
    {
        var sessionId = Guid.NewGuid();

        var taskRepository =
            new Mock<IAgentTaskRepository>();

        var dependencyRepository =
            new Mock<ITaskDependencyRepository>();

        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "root",
                "Root",
                "Root task."),

            new PlannedTask(
                "child",
                "Child",
                "Child task.",
                "root")
            },
            Array.Empty<PlannedDependency>());

        var service = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await service.ApplyAsync(
            sessionId,
            plan);

        var root = result.Single(
            x => x.Title == "Root");

        var child = result.Single(
            x => x.Title == "Child");

        Assert.Null(root.ParentTaskId);

        Assert.Equal(
            root.Id,
            child.ParentTaskId);
    }

    [Fact]
    public async Task Rejects_child_when_parent_has_not_been_created_yet()
    {
        var sessionId = Guid.NewGuid();

        var taskRepository =
            new Mock<IAgentTaskRepository>();

        var dependencyRepository =
            new Mock<ITaskDependencyRepository>();

        var plan = new PlanningResult(
            new[]
            {
            new PlannedTask(
                "child",
                "Child",
                "Child task.",
                "root"),

            new PlannedTask(
                "root",
                "Root",
                "Root task.")
            },
            Array.Empty<PlannedDependency>());

        var service = new PlanApplicationService(
            taskRepository.Object,
            dependencyRepository.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ApplyAsync(
                sessionId,
                plan));
    }
}