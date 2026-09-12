using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Domain.Tests;

/// <summary>
/// Unit tests for <see cref="AgentFlow.Domain.Entities.AgentTask"/> lifecycle and validation behavior.
/// </summary>
public class AgentTaskTests
{
    [Fact]
    public void New_task_starts_as_pending()
    {
        var sessionId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Test task",
            "Test description");

        Assert.Equal(
            TaskState.Pending,
            task.State);
    }

    [Fact]
    public void Pending_task_can_be_queued()
    {
        var task = CreateTask();

        task.Queue();

        Assert.Equal(
            TaskState.Queued,
            task.State);
    }

    [Fact]
    public void Queued_task_can_be_started()
    {
        var task = CreateTask();

        task.Queue();
        task.Start();

        Assert.Equal(
            TaskState.Running,
            task.State);
    }

    [Fact]
    public void Running_task_can_succeed()
    {
        var task = CreateTask();

        task.Queue();
        task.Start();
        task.Succeed();

        Assert.Equal(
            TaskState.Succeeded,
            task.State);
    }

    private static AgentTask CreateTask()
    {
        return new AgentTask(
            Guid.NewGuid(),
            "Test task",
            "Test description");
    }

    [Fact]
    public void Pending_task_cannot_be_started()
    {
        var task = CreateTask();

        Assert.Throws<InvalidOperationException>(
            () => task.Start());
    }

    [Fact]
    public void Pending_task_cannot_succeed()
    {
        var task = CreateTask();

        Assert.Throws<InvalidOperationException>(
            () => task.Succeed());
    }

    [Fact]
    public void Succeeded_task_cannot_be_started_again()
    {
        var task = CreateTask();

        task.Queue();
        task.Start();
        task.Succeed();

        Assert.Throws<InvalidOperationException>(
            () => task.Start());
    }

    [Fact]
    public void Pending_task_can_be_cancelled()
    {
        var task = CreateTask();

        task.Cancel();

        Assert.Equal(
            TaskState.Cancelled,
            task.State);
    }

    [Fact]
    public void Running_task_can_be_cancelled()
    {
        var task = CreateTask();

        task.Queue();
        task.Start();
        task.Cancel();

        Assert.Equal(
            TaskState.Cancelled,
            task.State);
    }

    [Fact]
    public void Succeeded_task_cannot_be_cancelled()
    {
        var task = CreateTask();

        task.Queue();
        task.Start();
        task.Succeed();

        Assert.Throws<InvalidOperationException>(
            () => task.Cancel());
    }

    [Fact]
    public void Empty_session_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => new AgentTask(
                Guid.Empty,
                "Test task",
                "Test description"));
    }

    [Fact]
    public void Empty_title_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => new AgentTask(
                Guid.NewGuid(),
                "",
                "Test description"));
    }

    [Fact]
    public void Empty_description_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => new AgentTask(
                Guid.NewGuid(),
                "Test task",
                ""));
    }

    [Fact]
    public void Start_FirstAttempt_IncrementsAttemptCount()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Research",
            "Research requirements");

        task.Queue();
        task.Start();

        Assert.Equal(1, task.AttemptCount);
    }

    [Fact]
    public void Retry_AfterFailure_QueuesTaskAgain()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Research",
            "Research requirements");

        task.Queue();
        task.Start();
        task.Fail();

        task.Retry();

        Assert.Equal(
            TaskState.Queued,
            task.State);

        Assert.Equal(1, task.AttemptCount);
    }
}