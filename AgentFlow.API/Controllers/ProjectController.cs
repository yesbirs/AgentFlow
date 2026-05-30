using AgentFlow.API.Models;
using AgentFlow.API.Services;
using Microsoft.AspNetCore.Mvc;
using AgentFlow.API.Options;
using System.Threading;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly PaginationOptions _paginationOptions;

        public ProjectController(IProjectService projectService, Microsoft.Extensions.Options.IOptions<PaginationOptions> paginationOptions)
        {
            _projectService = projectService;
            _paginationOptions = paginationOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProjectQueryParameters projectQueryParameters, CancellationToken cancellationToken)
        {
            if (projectQueryParameters.PageNumber < 1) projectQueryParameters.PageNumber = 1;
            if (projectQueryParameters.PageSize < 1) projectQueryParameters.PageSize = _paginationOptions.DefaultPageSize;
            if (projectQueryParameters.PageSize > _paginationOptions.MaxPageSize) projectQueryParameters.PageSize = _paginationOptions.MaxPageSize;

            var projects = await _projectService.GetAllProjects(projectQueryParameters, cancellationToken);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var project = await _projectService.GetProjectById(id, cancellationToken);
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
        {
            var createdProject = await _projectService.CreateProject(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdProject.Id }, createdProject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            var updatedProject = await _projectService.UpdateProject(id, request, cancellationToken);
            return Ok(updatedProject);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _projectService.DeleteProject(id, cancellationToken);
            return NoContent();
        }
    }
}