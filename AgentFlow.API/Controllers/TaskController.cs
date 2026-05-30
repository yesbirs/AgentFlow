using AgentFlow.API.Exceptions;
using AgentFlow.API.Models;
using AgentFlow.API.Services;
using Microsoft.AspNetCore.Mvc;
using AgentFlow.API.Options;
using System.Threading;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
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

            var tasks = await _taskService.GetFilteredTasks(parameters, cancellationToken);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var task = await _taskService.GetTaskById(id, cancellationToken);
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
        {
            var createdTask = await _taskService.CreateTask(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest updatedTask, CancellationToken cancellationToken)
        {
            var existingTask = await _taskService.GetTaskById(id, cancellationToken);
            if (existingTask == null)
            {
                throw new TaskNotFoundException(id);
            }
            var updated = await _taskService.UpdateTask(id, updatedTask, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _taskService.DeleteTask(id, cancellationToken);
            return NoContent();
        }
    }
}