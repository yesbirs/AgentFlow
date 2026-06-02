using AgentFlow.API.Exceptions;
using AgentFlow.API.Models;
using AgentFlow.API.Options;
using AgentFlow.API.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [EnableRateLimiting("GlobalLimiter")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly PaginationOptions _paginationOptions;

        public TaskController(ITaskService taskService, Microsoft.Extensions.Options.IOptions<PaginationOptions> paginationOptions)
        {
            _taskService = taskService;
            _paginationOptions = paginationOptions.Value;
        }

        // GET /tasks
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TaskQueryParameters parameters, CancellationToken cancellationToken)
        {
            // Apply pagination options
            if (parameters.PageNumber < 1) parameters.PageNumber = 1;
            if (parameters.PageSize < 1) parameters.PageSize = _paginationOptions.DefaultPageSize;
            if (parameters.PageSize > _paginationOptions.MaxPageSize) parameters.PageSize = _paginationOptions.MaxPageSize;

            // Set CreatedByUserId for non-admins so users only see their own tasks by default
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                parameters.CreatedByUserId = userId;
            }

            var tasks = await _taskService.GetFilteredTasks(parameters, cancellationToken);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var task = await _taskService.GetTaskById(id, userId, isAdmin, cancellationToken);
                if (task == null)
                {
                    throw new TaskNotFoundException(id);
                }

                return Ok(task);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var createdTask = await _taskService.CreateTask(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest updatedTask, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var updated = await _taskService.UpdateTask(id, updatedTask, userId, isAdmin, cancellationToken);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (TaskNotFoundException)
            {
                throw;
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _taskService.DeleteTask(id, cancellationToken);
            return NoContent();
        }
    }
}