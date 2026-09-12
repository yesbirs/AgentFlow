using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Queue;
using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;
using Moq;

namespace AgentFlow.Application.Tests.Execution;

/// <summary>
/// Unit tests for <see cref="AgentFlow.Application.Execution.TaskWorker"/>
/// verifying processing behavior and error handling.
/// </summary>
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
        var unitOfWork = new Mock<IUnitOfWork>();
        var retryPolicy = new RetryPolicy(3);
        FakeWorkItem workItem = new FakeWorkItem(task.Id);

        queue
            .Setup(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(workItem);

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
            executor.Object, unitOfWork.Object, retryPolicy);

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
        var unitOfWork = new Mock<IUnitOfWork>();
        var retryPolicy = new RetryPolicy(3);
        FakeWorkItem workItem = new FakeWorkItem(taskId);

        queue
            .Setup(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(workItem);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                taskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentTask?)null);

        var worker = new TaskWorker(
            queue.Object,
            taskRepository.Object,
            executor.Object, unitOfWork.Object, retryPolicy);

        await worker.ProcessNextAsync();

        executor.Verify(
            x => x.ExecuteAsync(
                It.IsAny<AgentTask>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessNextAsync_ExecutorFails_TaskFails()
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
        var unitOfWork = new Mock<IUnitOfWork>();
        var retryPolicy = new RetryPolicy(3);
        FakeWorkItem workItem = new FakeWorkItem(task.Id);

        queue
            .Setup(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(workItem);

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
            executor.Object, unitOfWork.Object, retryPolicy);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.ProcessNextAsync());

        Assert.Equal(AgentFlow.Domain.Enums.TaskState.Queued, task.State);
        unitOfWork.Verify(
            x => x.SaveChangesAsync(
               It.IsAny<CancellationToken>()),
                Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessNextAsync_ExecutorFailsAndRetryAllowed_RequeuesTask()
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
        var unitOfWork = new Mock<IUnitOfWork>();
        FakeWorkItem workItem = new FakeWorkItem(task.Id);

        queue
           .Setup(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(workItem);

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

        var retryPolicy = new RetryPolicy(3);

        var worker = new TaskWorker(
            queue.Object,
            taskRepository.Object,
            executor.Object,
            unitOfWork.Object,
            retryPolicy);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.ProcessNextAsync());

        Assert.Equal(
            AgentFlow.Domain.Enums.TaskState.Queued,
            task.State);

        Assert.Equal(1, task.AttemptCount);
    }

    [Fact]
    public async Task ProcessNextAsync_MaxAttemptsReached_DoesNotRequeueTask()
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
        var unitOfWork = new Mock<IUnitOfWork>();
        FakeWorkItem workItem = new FakeWorkItem(task.Id);

        queue
            .Setup(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(workItem);

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

        var retryPolicy = new RetryPolicy(1);

        var worker = new TaskWorker(
            queue.Object,
            taskRepository.Object,
            executor.Object,
            unitOfWork.Object,
            retryPolicy);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => worker.ProcessNextAsync());

        Assert.Equal(
            AgentFlow.Domain.Enums.TaskState.Failed,
            task.State);

        Assert.Equal(1, task.AttemptCount);

        queue.Verify(
            x => x.EnqueueAsync(
                task.Id,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    internal sealed class FakeWorkItem : IWorkItem
    {
        public Guid TaskId { get; }

        public bool IsCompleted { get; private set; }

        public bool IsAbandoned { get; private set; }

        public bool IsDeadLettered { get; private set; }

        public FakeWorkItem(Guid taskId)
        {
            TaskId = taskId;
        }

        public Task CompleteAsync(
            CancellationToken cancellationToken = default)
        {
            IsCompleted = true;
            return Task.CompletedTask;
        }

        public Task AbandonAsync(
            CancellationToken cancellationToken = default)
        {
            IsAbandoned = true;
            return Task.CompletedTask;
        }

        public Task DeadLetterMessageAsync(
            CancellationToken cancellationToken = default)
        {
            IsAbandoned = true;
            return Task.CompletedTask;
        }
    }
}