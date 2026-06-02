using AgentFlow.API.Data;
using AgentFlow.API.Exceptions;
using AgentFlow.API.Models;
using AgentFlow.API.Repositories;
using AgentFlow.API.Services;
using System.Threading;

namespace AgentFlow.Api.Services;

public class PostgreSqlTaskService : ITaskService
{
    private readonly ILogger<PostgreSqlTaskService> _logger;
    private readonly ITaskRepository _taskRepository;

    public PostgreSqlTaskService(ITaskRepository taskRepository, ILogger<PostgreSqlTaskService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<AgentTask> CreateTask(CreateTaskRequest request, string? userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating task in database: {TaskName}", request.Name);

        var task = new AgentTask
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Priority = request.Priority,
            ScheduledAt = request.ScheduledAt,
            CreatedAt = DateTime.UtcNow,
            Status = "pending",
            CreatedByUserId = userId
        };

        await _taskRepository.CreateTask(task, cancellationToken);

        _logger.LogInformation("Task saved to database with ID: {TaskId}", task.Id);
        return task;
    }

    public async Task<AgentTask?> GetTaskById(Guid id, string? userId, bool isAdmin, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching task from database: {TaskId}", id);
        var task = await _taskRepository.GetTask(id, cancellationToken);
        if (task == null)
        {
            return null;
        }

        if (!isAdmin && task.CreatedByUserId != userId)
        {
            // Not authorized
            throw new UnauthorizedAccessException("Not allowed to access this task");
        }

        return task;
    }

    public async Task<PagedResult<AgentTask>> GetFilteredTasks(TaskQueryParameters taskQueryParameters, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching filtered tasks: page {PageNumber}, size {PageSize}, status {Status}, priority {Priority}",
            taskQueryParameters.PageNumber,
            taskQueryParameters.PageSize,
            taskQueryParameters.Status,
            taskQueryParameters.Priority);

        List<AgentTask> filteredTasks = await _taskRepository.GetFilteredAsync(taskQueryParameters, cancellationToken);
        int totalCount = await _taskRepository.GetTotalTasksCount(taskQueryParameters, cancellationToken);

        return new PagedResult<AgentTask>
        {
            Data = filteredTasks,
            TotalCount = totalCount,
            PageNumber = taskQueryParameters.PageNumber,
            PageSize = taskQueryParameters.PageSize
        };
    }

    public async Task<AgentTask> UpdateTask(Guid id, UpdateTaskRequest request, string? userId, bool isAdmin, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating task in database: {TaskId}", id);

        var task = await _taskRepository.GetTask(id, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found in database", id);
            throw new TaskNotFoundException(id);
        }

        if (!isAdmin && task.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("Not allowed to update this task");
        }

        task.Name = request.Name;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.ScheduledAt = request.ScheduledAt;
        task.Status = request.Status;

        await _taskRepository.UpdateTask(task, cancellationToken);

        _logger.LogInformation("Task {TaskId} updated in database", id);
        return task;
    }

    public async Task DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting task from database: {TaskId}", id);

        var task = await _taskRepository.GetTask(id, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found in database for deletion", id);
            throw new TaskNotFoundException(id);
        }
        await _taskRepository.DeleteTask(task, cancellationToken);

        _logger.LogInformation("Task {TaskId} deleted from database", id);
    }

    public async Task<int> GetTotalTasksCount(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting total tasks count from database");
        // Return unfiltered total count
        return await _taskRepository.GetTotalTasksCount(new TaskQueryParameters(), cancellationToken);
    }
}