using AgentFlow.Application.Execution;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Application.Tests.Execution;

/// <summary>
/// Unit tests for the <see cref="AgentFlow.Application.Execution.RetryPolicy"/> behaviour.
/// </summary>
public class RetryPolicyTests
{
    [Fact]
    public void ShouldRetry_FailedTaskBelowLimit_ReturnsTrue()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Research",
            "Research requirements");

        task.Queue();
        task.Start();
        task.Fail();

        var policy = new RetryPolicy(3);

        Assert.True(policy.ShouldRetry(task));
    }

    [Fact]
    public void ShouldRetry_TaskReachedMaximumAttempts_ReturnsFalse()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Research",
            "Research requirements");

        task.Queue();
        task.Start();
        task.Fail();

        task.Retry();

        task.Start();
        task.Fail();

        task.Retry();

        task.Start();
        task.Fail();

        var policy = new RetryPolicy(3);

        Assert.False(policy.ShouldRetry(task));
    }

    [Fact]
    public void ShouldRetry_SucceededTask_ReturnsFalse()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Research",
            "Research requirements");

        task.Queue();
        task.Start();
        task.Succeed();

        var policy = new RetryPolicy(3);

        Assert.False(policy.ShouldRetry(task));
    }
}