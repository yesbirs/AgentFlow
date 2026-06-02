using AgentFlow.API.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class HelloController : ControllerBase
    {
        // GET /hello
        [HttpGet]
        public IActionResult Get()
        {
            var response = new
            {
                Message = "Hello AgentFlow!",
                Date = DateTime.UtcNow,
                Status = "Day 1 Complete"
            };

            // Returns HTTP 200 with JSON
            return Ok(response);
        }

        // POST /hello/tasks
        [HttpPost("tasks")]
        public IActionResult CreateTask([FromBody] CreateTaskRequest request)
        {
            // Simulate creating a task (no database yet — that's Month 2)
            var createdTask = new
            {
                Id = Guid.NewGuid(),           // Generate a fake ID
                Name = request.Name,
                Description = request.Description,
                Priority = request.Priority,
                ScheduledAt = request.ScheduledAt,
                CreatedAt = DateTime.UtcNow,
                Status = "pending"
            };

            // 201 Created is the correct status for successful POST
            return Created($"/hello/tasks/{createdTask.Id}", createdTask);
        }
    }
}