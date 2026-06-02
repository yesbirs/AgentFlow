using AgentFlow.API.Controllers;
using AgentFlow.API.Models;
using AgentFlow.API.Options;
using AgentFlow.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AgentFlow.Api.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IOptions<PaginationOptions>> _paginationOptionsMock;
    private readonly Mock<ILogger<TaskController>> _loggerMock;
    private readonly TaskController _controller;

    public TasksControllerTests()
    {
        // Arrange: Create mocks for all dependencies
        _taskServiceMock = new Mock<ITaskService>();
        _paginationOptionsMock = new Mock<IOptions<PaginationOptions>>();
        _loggerMock = new Mock<ILogger<TaskController>>();

        // Setup pagination options
        _paginationOptionsMock.Setup(x => x.Value).Returns(new PaginationOptions
        {
            DefaultPageSize = 10,
            MaxPageSize = 100
        });

        // Create the controller with mocked dependencies
        _controller = new TaskController(
            _taskServiceMock.Object,
            _paginationOptionsMock.Object);
    }

    [Fact]
    public async Task GetById_WithExistingTask_ReturnsOkWithTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var expectedTask = new AgentTask
        {
            Id = taskId,
            Name = "Test task",
            Priority = "high",
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _taskServiceMock
            .Setup(x => x.GetTaskById(taskId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTask);

        // Act
        var result = await _controller.GetById(taskId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<AgentTask>().Subject;
        returnedTask.Id.Should().Be(taskId);
        returnedTask.Name.Should().Be("Test task");
    }

    [Fact]
    public async Task GetById_WithNonExistingTask_ReturnsOkWithNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        _taskServiceMock
            .Setup(x => x.GetTaskById(taskId, It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentTask?)null);

        // Act
        var result = await _controller.GetById(taskId, CancellationToken.None);

        // Assert
        // Note: Your controller returns Ok(null) currently. In a real app,
        // the service throws TaskNotFoundException which middleware catches.
        // But for unit testing the controller directly, we test what it returns.
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Name = "New task",
            Priority = "medium"
        };

        var createdTask = new AgentTask
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Priority = request.Priority,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _taskServiceMock
            .Setup(x => x.CreateTask(request, It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(TaskController.GetById));
        createdResult.RouteValues?["id"].Should().Be(createdTask.Id);

        var returnedTask = createdResult.Value.Should().BeOfType<AgentTask>().Subject;
        returnedTask.Name.Should().Be("New task");
    }

    [Fact]
    public async Task Delete_WithValidID_ReturnsNoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _taskServiceMock
            .Setup(x => x.DeleteTask(taskId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        // Act
        var result = await _controller.Delete(taskId, CancellationToken.None);
        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}