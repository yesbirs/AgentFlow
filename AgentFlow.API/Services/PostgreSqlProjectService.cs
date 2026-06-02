using AgentFlow.API.Exceptions;
using AgentFlow.API.Models;
using AgentFlow.API.Repositories;
using System.Threading;

namespace AgentFlow.API.Services
{
    public class PostgreSqlProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<PostgreSqlProjectService> _logger;

        public PostgreSqlProjectService(IProjectRepository projectRepository, ILogger<PostgreSqlProjectService> logger)
        {
            _projectRepository = projectRepository;
            _logger = logger;
        }

        public async Task<Project> CreateProject(CreateProjectRequest request, string? userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating project with name: {ProjectName}", request.Name);
            var newProject = new Project
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Status = "Active",
                OwnerEmail = request.OwnerEmail,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId
            };
            await _projectRepository.CreateProject(newProject, cancellationToken);
            _logger.LogInformation("Project created with ID: {ProjectId}", newProject.Id);
            return newProject;
        }

        public async Task DeleteProject(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting project with ID: {ProjectId}", id);
            var project = await GetProjectById(id, cancellationToken);
            if (project != null)
            {
                await _projectRepository.DeleteProject(project, cancellationToken);
                _logger.LogInformation("Project deleted with ID: {ProjectId}", id);
            }
            else
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found for deletion", id);
                throw new ProjectNotFoundException(id);
            }
        }

        public async Task<PagedResult<Project>> GetAllProjects(ProjectQueryParameters projectQueryParameters, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all projects from database");
            var filteredProjects = await _projectRepository.GetAllProjects(projectQueryParameters, cancellationToken);

            return new PagedResult<Project>
            {
                Data = filteredProjects,
                TotalCount = await _projectRepository.GetTotalProjectCounts(projectQueryParameters, cancellationToken),
                PageNumber = projectQueryParameters.PageNumber,
                PageSize = projectQueryParameters.PageSize
            };
        }

        public async Task<Project?> GetProjectById(Guid id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching project with ID: {ProjectId}", id);
            var project = await _projectRepository.GetProjectById(id, cancellationToken);
            if (project != null)
            {
                return project;
            }
            else
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found", id);
                throw new ProjectNotFoundException(id);
            }
        }

        public async Task<Project?> UpdateProject(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating project with ID: {ProjectId}", id);
            var project = await _projectRepository.GetProjectById(id, cancellationToken);
            if (project != null)
            {
                project.Name = request.Name;
                project.Description = request.Description;
                project.Status = request.Status;
                await _projectRepository.UpdateProject(project, cancellationToken);
                _logger.LogInformation("Project updated with ID: {ProjectId}", id);
                return project;
            }
            else
            {
                _logger.LogWarning("Project with ID: {ProjectId} not found for update", id);
                throw new ProjectNotFoundException(id);
            }
        }
    }
}