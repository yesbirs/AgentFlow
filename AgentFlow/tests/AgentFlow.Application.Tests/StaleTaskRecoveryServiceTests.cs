using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Application.Tests;

public sealed class StaleTaskRecoveryServiceTests
{
    [Fact]
    public async Task RecoverAsync_WhenRetryAvailable_RequeuesTask()
    {
        var task = CreateRunningTask();
        var repository = new FakeTaskRepository();
        repository.StaleTasks.Add(task);

        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork, new RetryPolicy(3));

        await service.RecoverAsync();

        Assert.Equal(TaskState.Queued, task.State);

        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task RecoverAsync_WhenRetriesExhausted_LeavesTaskFailed()
    {
        var task = CreateRunningTask();
        task.Fail();
        task.Retry();
        task.Start();

        var repository = new FakeTaskRepository();
        repository.StaleTasks.Add(task);

        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork, new RetryPolicy(2));

        await service.RecoverAsync();

        Assert.Equal(TaskState.Failed, task.State);
        Assert.Equal(2, task.AttemptCount);
    }

    [Fact]
    public async Task RecoverAsync_WhenStaleTasksExist_SavesChanges()
    {
        var repository = new FakeTaskRepository();
        repository.StaleTasks.Add(CreateRunningTask());

        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(repository, unitOfWork, new RetryPolicy(3));

        await service.RecoverAsync();

        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    private static AgentTask CreateRunningTask()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Test task",
            "Test description");

        task.Queue();
        task.Start();

        return task;
    }

    private static StaleTaskRecoveryService CreateService(
        FakeTaskRepository repository,
        FakeUnitOfWork unitOfWork,
        RetryPolicy retryPolicy)
    {
        return new StaleTaskRecoveryService(
            repository,
            new FakeOutboxMessageRepository(),
            unitOfWork,
            retryPolicy,
            new FakeClock { UtcNow = DateTime.UtcNow });
    }

    private sealed class FakeTaskRepository : IAgentTaskRepository
    {
        public List<AgentTask> StaleTasks { get; } = [];

        public Task<IReadOnlyList<AgentTask>> GetStaleRunningTasksAsync(
            DateTime cutoff,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AgentTask>>(StaleTasks);
        }

        public Task AddAsync(
            AgentTask task,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<AgentTask?> GetTaskByIdAsync(
            Guid taskId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<AgentTask>> GetPendingTasksAsync(
            Guid sessionId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> TryClaimAsync(
            Guid taskId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<AgentTask>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task CommitTransactionAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RollbackTransactionAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}