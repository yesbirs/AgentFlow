using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;
using Moq;

namespace AgentFlow.Application.Tests.Execution;

public class TaskWorkerTests
{
    [Fact]
    public async Task ProcessNextAsync_SuccessfulExecution_TaskSucceeds()
    {
        var sessionId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");
        task.Queue();
        var queue = new Mock<IWorkQueue>();
        var taskRepository = new Mock<IAgentTaskRepository>();
        var executor = new Mock<ITaskExecutor>();

        queue
            .Setup(x => x.DequeueAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task.Id);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        executor
            .Setup(x => x.ExecuteAsync(
                task,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var worker = new TaskWorker(
            queue.Object,
            taskRepository.Object,
            executor.Object);

        await worker.ProcessNextAsync();

        Assert.Equal(
            AgentFlow.Domain.Enums.TaskState.Succeeded,
            task.State);

        executor.Verify(
            x => x.ExecuteAsync(
                task,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessNextAsync_TaskDoesNotExist_DoesNothing()
    {
        var taskId = Guid.NewGuid();

        var queue = new Mock<IWorkQueue>();
        var taskRepository = new Mock<IAgentTaskRepository>();
        var executor = new Mock<ITaskExecutor>();

        queue
            .Setup(x => x.DequeueAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskId);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                taskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentTask?)null);

        var worker = new TaskWorker(
            queue.Object,
            taskRepository.Object,
            executor.Object);

        await worker.ProcessNextAsync();

        executor.Verify(
            x => x.ExecuteAsync(
                It.IsAny<AgentTask>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessNextAsync_ExecutorFails_TaskRemainsRunning()
    {
        var sessionId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Research",
            "Research requirements");
        task.Queue();
        var queue = new Mock<IWorkQueue>();
        var taskRepository = new Mock<IAgentTaskRepository>();
        var executor = new Mock<ITaskExecutor>();

        queue
            .Setup(x => x.DequeueAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task.Id);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        executor
            .Setup(x => x.ExecuteAsync(
                task,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException("Execution failed."));

        var worker = new TaskWorker(
            queue.Object,
            taskRepository.Object,
            executor.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.ProcessNextAsync());

        Assert.Equal(
            AgentFlow.Domain.Enums.TaskState.Running,
            task.State);
    }
}