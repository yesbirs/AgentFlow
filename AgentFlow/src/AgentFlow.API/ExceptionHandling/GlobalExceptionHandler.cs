using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgentFlow.API.ExceptionHandling;

/// <summary>
/// Global exception handler used by the API to convert known exceptions into HTTP responses.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GlobalExceptionHandler"/>.
    /// </summary>
    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts to handle the provided exception and write an appropriate HTTP response.
    /// Returns true when the exception was handled.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An unhandled exception occurred.");

        if (exception is ArgumentException argumentException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: argumentException.Message)
                .ExecuteAsync(httpContext);

            return true;
        }

        return false;
    }
}
