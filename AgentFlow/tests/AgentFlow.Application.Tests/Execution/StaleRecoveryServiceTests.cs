using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Abstractions.Time;
using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;
using Moq;

namespace AgentFlow.Application.Tests.Execution
{
    /// <summary>
    /// Unit tests for <see cref="AgentFlow.Application.Execution.StaleTaskRecoveryService"/>
    /// verifying stale task detection, recovery and retry behaviour.
    /// </summary>
    public class StaleRecoveryServiceTests
    {
        private static AgentTask CreateRunningTask()
        {
            var task = new AgentTask(
                Guid.NewGuid(),
                "Research",
                "Research requirements");

            task.Queue();
            task.Start();

            return task;
        }

        [Fact]
        public async Task RecoverAsync_NoStaleTasks_DoesNotSaveChanges()
        {
            var taskRepository = new Mock<IAgentTaskRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var retryPolicy = new RetryPolicy(3);
            var clock = new FakeClock { UtcNow = DateTime.UtcNow };

            taskRepository
                .Setup(x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentTask>());

            var service = new StaleTaskRecoveryService(
                taskRepository.Object,
                unitOfWork.Object,
                retryPolicy,
                clock);

            await service.RecoverAsync();

            unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RecoverAsync_StaleTaskBelowAttemptLimit_TaskIsRequeued()
        {
            var task = CreateRunningTask();

            var taskRepository = new Mock<IAgentTaskRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var retryPolicy = new RetryPolicy(3);
            var clock = new FakeClock { UtcNow = DateTime.UtcNow };

            taskRepository
                .Setup(x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentTask> { task });

            var service = new StaleTaskRecoveryService(
                taskRepository.Object,
                unitOfWork.Object,
                retryPolicy,
                clock);

            await service.RecoverAsync();

            Assert.Equal(TaskState.Queued, task.State);
            Assert.Equal(1, task.AttemptCount);

            unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RecoverAsync_StaleTaskAtAttemptLimit_TaskStaysFailed()
        {
            var task = CreateRunningTask();
            task.Fail();

            task.Retry();
            task.Start();
            task.Fail();

            task.Retry();
            task.Start();

            var taskRepository = new Mock<IAgentTaskRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var retryPolicy = new RetryPolicy(3);
            var clock = new FakeClock { UtcNow = DateTime.UtcNow };

            taskRepository
                .Setup(x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentTask> { task });

            var service = new StaleTaskRecoveryService(
                taskRepository.Object,
                unitOfWork.Object,
                retryPolicy,
                clock);

            await service.RecoverAsync();

            Assert.Equal(TaskState.Failed, task.State);
            Assert.Equal(3, task.AttemptCount);

            unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RecoverAsync_MultipleStaleTasks_EachTaskIsRecovered()
        {
            var retryableTask = CreateRunningTask();

            var exhaustedTask = CreateRunningTask();
            exhaustedTask.Fail();
            exhaustedTask.Retry();
            exhaustedTask.Start();
            exhaustedTask.Fail();
            exhaustedTask.Retry();
            exhaustedTask.Start();

            var taskRepository = new Mock<IAgentTaskRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var retryPolicy = new RetryPolicy(3);
            var clock = new FakeClock { UtcNow = DateTime.UtcNow };

            taskRepository
                .Setup(x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentTask>
                {
                    retryableTask,
                    exhaustedTask
                });

            var service = new StaleTaskRecoveryService(
                taskRepository.Object,
                unitOfWork.Object,
                retryPolicy,
                clock);

            await service.RecoverAsync();

            Assert.Equal(TaskState.Queued, retryableTask.State);
            Assert.Equal(TaskState.Failed, exhaustedTask.State);

            unitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RecoverAsync_Always_RequestsTasksOlderThanStaleThreshold()
        {
            var taskRepository = new Mock<IAgentTaskRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var retryPolicy = new RetryPolicy(3);
            var clock = new FakeClock { UtcNow = DateTime.UtcNow };
            taskRepository
                .Setup(x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentTask>());

            var service = new StaleTaskRecoveryService(
                taskRepository.Object,
                unitOfWork.Object,
                retryPolicy,
                clock);

            var before = clock.UtcNow - TimeSpan.FromMinutes(10);

            await service.RecoverAsync();

            var after = clock.UtcNow - TimeSpan.FromMinutes(10);

            taskRepository.Verify(
                x => x.GetStaleRunningTasksAsync(
                    It.Is<DateTime>(cutoff =>
                        cutoff >= before && cutoff <= after),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RecoverAsync_CancellationToken_IsForwardedToDependencies()
        {
            var task = CreateRunningTask();

            var taskRepository = new Mock<IAgentTaskRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var retryPolicy = new RetryPolicy(3);
            var clock = new FakeClock { UtcNow = DateTime.UtcNow };
            taskRepository
                .Setup(x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AgentTask> { task });

            var service = new StaleTaskRecoveryService(
                taskRepository.Object,
                unitOfWork.Object,
                retryPolicy,
                clock);

            using var cancellationTokenSource = new CancellationTokenSource();

            await service.RecoverAsync(cancellationTokenSource.Token);

            taskRepository.Verify(
                x => x.GetStaleRunningTasksAsync(
                    It.IsAny<DateTime>(),
                    cancellationTokenSource.Token),
                Times.Once);

            unitOfWork.Verify(
                x => x.SaveChangesAsync(cancellationTokenSource.Token),
                Times.Once);
        }
    }
}