using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Scheduling;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;
using Moq;

namespace AgentFlow.Application.Tests.Scheduling;

public class SchedulerTests
{
    [Fact]
    public async Task FindReadyTasksAsync_TaskWithoutDependencies_ReturnsTask()
    {
        var sessionId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        taskRepository
            .Setup(x => x.GetPendingTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentTask> { task });

        dependencyRepository
            .Setup(x => x.GetDependencyTaskIdsAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        var scheduler = new Scheduler(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await scheduler.FindReadyTasksAsync(sessionId);

        Assert.Single(result);
        Assert.Equal(task.Id, result[0].Id);
    }

    [Fact]
    public async Task FindReadyTasksAsync_DependencyPending_DoesNotReturnTask()
    {
        var sessionId = Guid.NewGuid();

        var dependency = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        var task = new AgentTask(
            sessionId,
            "Design",
            "Design solution");

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        taskRepository
            .Setup(x => x.GetPendingTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentTask> { task });

        dependencyRepository
            .Setup(x => x.GetDependencyTaskIdsAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { dependency.Id });

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                dependency.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependency);

        var scheduler = new Scheduler(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await scheduler.FindReadyTasksAsync(sessionId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task FindReadyTasksAsync_DependencySucceeded_ReturnsTask()
    {
        var sessionId = Guid.NewGuid();

        var dependency = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        dependency.Queue();
        dependency.Start();
        dependency.Succeed();

        var task = new AgentTask(
            sessionId,
            "Design",
            "Design solution");

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        taskRepository
            .Setup(x => x.GetPendingTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentTask> { task });

        dependencyRepository
            .Setup(x => x.GetDependencyTaskIdsAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { dependency.Id });

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                dependency.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependency);

        var scheduler = new Scheduler(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await scheduler.FindReadyTasksAsync(sessionId);

        Assert.Single(result);
        Assert.Equal(task.Id, result[0].Id);
    }

    [Fact]
    public async Task FindReadyTasksAsync_MultipleDependencies_AnyIncomplete_DoesNotReturnTask()
    {
        var sessionId = Guid.NewGuid();

        var dependency1 = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        dependency1.Queue();
        dependency1.Start();
        dependency1.Succeed();

        var dependency2 = new AgentTask(
            sessionId,
            "Requirements",
            "Gather requirements");

        var task = new AgentTask(
            sessionId,
            "Design",
            "Design solution");

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository = new Mock<ITaskDependencyRepository>();

        taskRepository
            .Setup(x => x.GetPendingTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentTask> { task });

        dependencyRepository
            .Setup(x => x.GetDependencyTaskIdsAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<Guid>
                {
                dependency1.Id,
                dependency2.Id
                });

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                dependency1.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependency1);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                dependency2.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependency2);

        var scheduler = new Scheduler(
            taskRepository.Object,
            dependencyRepository.Object);

        var result = await scheduler.FindReadyTasksAsync(sessionId);

        Assert.Empty(result);
    }
}