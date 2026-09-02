using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Sessions.CreateTask;
using AgentFlow.Application.Sessions.CreateTaskDependency;
using AgentFlow.Domain.Entities;
using Moq;

namespace AgentFlow.Application.Tests.Sessions.CreateTask;

public class CreateTaskHandlerTests
{
    private readonly Mock<IUnitOfWork> unitOfWork = new Mock<IUnitOfWork>();

    [Fact]
    public async Task Creates_task_when_session_exists()
    {
        var sessionId = Guid.NewGuid();

        var sessionRepository = new Mock<ISessionRepository>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        sessionRepository
            .Setup(x => x.IsExistAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateTaskHandler(
            sessionRepository.Object,
            taskRepository.Object,
            unitOfWork.Object);

        var command = new CreateTaskCommand(
            sessionId,
            "Test task",
            "Test description");

        var result = await handler.Handle(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal("Test task", result.Title);
        Assert.Equal("Test description", result.Description);
        taskRepository.Verify(x => x.AddAsync(
                It.Is<AgentTask>(t =>
                    t.SessionId == sessionId &&
                    t.Title == "Test task" &&
                    t.Description == "Test description"),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task Rejects_task_when_session_does_not_exist()
    {
        var sessionId = Guid.NewGuid();

        var sessionRepository = new Mock<ISessionRepository>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        sessionRepository
            .Setup(x => x.IsExistAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateTaskHandler(
            sessionRepository.Object,
            taskRepository.Object,
            unitOfWork.Object);

        var command = new CreateTaskCommand(
            sessionId,
            "Test task",
            "Test description");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command));

        taskRepository.Verify(
            x => x.AddAsync(
                It.IsAny<AgentTask>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Rejects_task_when_parent_does_not_exist()
    {
        var sessionId = Guid.NewGuid();
        var parentTaskId = Guid.NewGuid();

        var sessionRepository = new Mock<ISessionRepository>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        sessionRepository
            .Setup(x => x.IsExistAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                parentTaskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentTask?)null);

        var handler = new CreateTaskHandler(
            sessionRepository.Object,
            taskRepository.Object, unitOfWork.Object);

        var command = new CreateTaskCommand(
            sessionId,
            "Child task",
            "Description",
            parentTaskId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command));

        taskRepository.Verify(
            x => x.AddAsync(
                It.IsAny<AgentTask>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Creates_root_task_without_parent()
    {
        var sessionId = Guid.NewGuid();

        var sessionRepository = new Mock<ISessionRepository>();
        var taskRepository = new Mock<IAgentTaskRepository>();

        sessionRepository
            .Setup(x => x.IsExistAsync(
                sessionId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateTaskHandler(
            sessionRepository.Object,
            taskRepository.Object,
            unitOfWork.Object);

        var command = new CreateTaskCommand(
            sessionId,
            "Root task",
            "Root description");

        var result = await handler.Handle(command);

        Assert.Null(result.ParentTaskId);

        taskRepository.Verify(
            x => x.AddAsync(
                It.Is<AgentTask>(t =>
                    t.ParentTaskId == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Creates_dependency_when_tasks_are_valid()
    {
        var sessionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var dependencyTaskId = Guid.NewGuid();

        var task = new AgentTask(
            sessionId,
            "Task",
            "Description");

        var dependencyTask = new AgentTask(
            sessionId,
            "Dependency",
            "Description");

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository =
            new Mock<ITaskDependencyRepository>();

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                taskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                dependencyTaskId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependencyTask);

        var handler = new CreateTaskDependencyHandler(
            taskRepository.Object,
            dependencyRepository.Object,
            unitOfWork.Object);

        var command = new CreateTaskDependencyCommand(
            taskId,
            dependencyTaskId);

        var result = await handler.Handle(command);

        Assert.Equal(taskId, result.TaskId);
        Assert.Equal(
            dependencyTaskId,
            result.DependsOnTaskId);

        dependencyRepository.Verify(
            x => x.AddAsync(
                It.Is<TaskDependency>(d =>
                    d.TaskId == taskId &&
                    d.DependsOnTaskId == dependencyTaskId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Rejects_dependency_between_different_sessions()
    {
        var task = new AgentTask(
            Guid.NewGuid(),
            "Task",
            "Description");

        var dependencyTask = new AgentTask(
            Guid.NewGuid(),
            "Dependency",
            "Description");

        var taskRepository = new Mock<IAgentTaskRepository>();
        var dependencyRepository =
            new Mock<ITaskDependencyRepository>();

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                task.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        taskRepository
            .Setup(x => x.GetTaskByIdAsync(
                dependencyTask.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dependencyTask);

        var handler = new CreateTaskDependencyHandler(
            taskRepository.Object,
            dependencyRepository.Object,
            unitOfWork.Object);

        var command = new CreateTaskDependencyCommand(
            task.Id,
            dependencyTask.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command));

        dependencyRepository.Verify(
            x => x.AddAsync(
                It.IsAny<TaskDependency>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}