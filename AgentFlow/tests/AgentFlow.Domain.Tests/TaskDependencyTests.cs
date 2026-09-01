using AgentFlow.Domain.Entities;

namespace AgentFlow.Domain.Tests;

public class TaskDependencyTests
{
    [Fact]
    public void Task_can_depend_on_another_task()
    {
        var taskId = Guid.NewGuid();
        var dependencyTaskId = Guid.NewGuid();

        var dependency = new TaskDependency(
            taskId,
            dependencyTaskId);

        Assert.Equal(
            taskId,
            dependency.TaskId);

        Assert.Equal(
            dependencyTaskId,
            dependency.DependsOnTaskId);
    }

    [Fact]
    public void Task_cannot_depend_on_itself()
    {
        var taskId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(
            () => new TaskDependency(
                taskId,
                taskId));
    }

    [Fact]
    public void Empty_task_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => new TaskDependency(
                Guid.Empty,
                Guid.NewGuid()));
    }

    [Fact]
    public void Empty_dependency_task_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(
            () => new TaskDependency(
                Guid.NewGuid(),
                Guid.Empty));
    }
}