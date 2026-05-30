using AgentFlow.API.Exceptions;
using AgentFlow.API.Models;
using AgentFlow.API.Options;
using AgentFlow.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AgentFlow.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowDefinitionsController : ControllerBase
    {
        private readonly IWorkflowDefinitionService _workflowDefinitionService;
        private readonly PaginationOptions _paginationOptions;
        private readonly ILogger<WorkflowDefinitionsController> _logger;

        public WorkflowDefinitionsController(
            IWorkflowDefinitionService workflowDefinitionService,
            IOptions<PaginationOptions> paginationOptions,
            ILogger<WorkflowDefinitionsController> logger)
        {
            _workflowDefinitionService = workflowDefinitionService;
            _paginationOptions = paginationOptions.Value;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated, filterable, and sortable workflow definitions
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? isActive = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] string sortOrder = "desc",
            CancellationToken cancellationToken = default)
        {
            // Apply pagination options
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = _paginationOptions.DefaultPageSize;
            if (pageSize > _paginationOptions.MaxPageSize) pageSize = _paginationOptions.MaxPageSize;

            var parameters = new WorkflowDefinitionQueryParameters
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                IsActive = isActive,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _workflowDefinitionService.GetFilteredAsync(parameters, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a single workflow definition by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var workflowDefinition = await _workflowDefinitionService.GetByIdAsync(id, cancellationToken);

            if (workflowDefinition == null)
            {
                throw new WorkflowDefinitionNotFoundException(id);
            }

            return Ok(workflowDefinition);
        }

        /// <summary>
        /// Create a new workflow definition
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkFlowDefinitionRequest request,
            CancellationToken cancellationToken)
        {
            var createdWorkflow = await _workflowDefinitionService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdWorkflow.Id }, createdWorkflow);
        }

        /// <summary>
        /// Update an existing workflow definition
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateWorkflowDefinitionRequest request,
            CancellationToken cancellationToken)
        {
            var updatedWorkflow = await _workflowDefinitionService.UpdateAsync(id, request, cancellationToken);
            return Ok(updatedWorkflow);
        }

        /// <summary>
        /// Soft delete a workflow definition (sets IsActive to false)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _workflowDefinitionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Reactivate a soft-deleted workflow definition
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            var activatedWorkflow = await _workflowDefinitionService.ActivateAsync(id, cancellationToken);
            return Ok(activatedWorkflow);
        }
    }
}
