using AgentFlow.API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AgentFlow.API.MiddleWares
{
    public class GlobalExceptionMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleWare> _logger;

        public GlobalExceptionMiddleWare(RequestDelegate next, ILogger<GlobalExceptionMiddleWare> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Pass the request to the next middleware (or controller)
                await _next(context);
            }
            catch (ProjectNotFoundException ex)
            {
                _logger.LogWarning(ex, "Handled ProjectNotFoundException");
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/problem+json";
                var problemDetails = new ProblemDetails
                {
                    Type = "https://example.com/probs/project-not-found",
                    Title = "Project Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
            catch (TaskNotFoundException ex)
            {
                _logger.LogWarning(ex, "Handled TaskNotFoundException");

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/problem+json";
                var problemDetails = new ProblemDetails
                {
                    Type = "https://example.com/probs/task-not-found",
                    Title = "Task Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
            catch (WorkflowDefinitionNotFoundException ex)
            {
                _logger.LogWarning(ex, "Handled WorkflowDefinitionNotFoundException");

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/problem+json";
                var problemDetails = new ProblemDetails
                {
                    Type = "https://example.com/probs/workflow-definition-not-found",
                    Title = "Workflow Definition Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = ex.Message
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                // Never expose stack traces to clients in production
                var problemDetails = new ProblemDetails
                {
                    Type = "https://example.com/probs/internal-server-error",
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later."
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
        }
    }
}