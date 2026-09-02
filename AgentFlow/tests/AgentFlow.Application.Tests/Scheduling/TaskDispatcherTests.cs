using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Application.Scheduling;
using AgentFlow.Domain.Entities;
using Moq;

namespace AgentFlow.Application.Tests.Scheduling;

public class TaskDispatcherTests
{
    [Fact]
    public async Task DispatchReadyTasksAsync_ClaimedTask_IsEnqueued()
    {
        var sessionId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        var scheduler = new Mock<IScheduler>();
        var queue = new Mock<IWorkQueue>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        scheduler
            .Setup(x => x.FindReadyTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentTask> { task });

        taskRepository
            .Setup(x => x.TryClaimAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dispatcher = new TaskDispatcher(
            scheduler.Object,
            queue.Object,
            taskRepository.Object);

        var result = await dispatcher.DispatchReadyTasksAsync(sessionId);

        Assert.Equal(1, result);

        queue.Verify(
            x => x.EnqueueAsync(
                task.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchReadyTasksAsync_TaskCannotBeClaimed_IsNotEnqueued()
    {
        var sessionId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        var scheduler = new Mock<IScheduler>();
        var queue = new Mock<IWorkQueue>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        scheduler
            .Setup(x => x.FindReadyTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentTask> { task });

        taskRepository
            .Setup(x => x.TryClaimAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var dispatcher = new TaskDispatcher(
            scheduler.Object,
            queue.Object,
            taskRepository.Object);

        var result = await dispatcher.DispatchReadyTasksAsync(sessionId);

        Assert.Equal(0, result);

        queue.Verify(
            x => x.EnqueueAsync(
                task.Id,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchReadyTasksAsync_MultipleClaimedTasks_EnqueuesAll()
    {
        var sessionId = Guid.NewGuid();

        var task1 = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");

        var task2 = new AgentTask(
            sessionId,
            "Design",
            "Design solution");

        var task3 = new AgentTask(
            sessionId,
            "Implementation",
            "Implement solution");

        var scheduler = new Mock<IScheduler>();
        var queue = new Mock<IWorkQueue>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        scheduler
            .Setup(x => x.FindReadyTasksAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new List<AgentTask>
                {
                task1,
                task2,
                task3
                });

        taskRepository
            .Setup(x => x.TryClaimAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dispatcher = new TaskDispatcher(
            scheduler.Object,
            queue.Object,
            taskRepository.Object);

        var result = await dispatcher.DispatchReadyTasksAsync(sessionId);

        Assert.Equal(3, result);

        queue.Verify(
            x => x.EnqueueAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }
}