using AgentFlow.Application.Sessions.CreateSession;
using AgentFlow.Application.Sessions.GetSessionById;
using AgentFlow.Application.Sessions.GetSessions;
using AgentFlow.Application.Sessions.PlanSession;

namespace AgentFlow.API.Endpoints
{
    public static class SessionEndpoints
    {
        public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost(
            "/api/sessions",
            async (
                CreateSessionCommand command,
                CreateSessionHandler handler,
                CancellationToken cancellationToken) =>
            {
                var session = await handler.Handle(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/sessions/{session.Id}",
                    session);
            });

            endpoints.MapGet("/api/sessions/{sessionId:guid}",
                async (
                    Guid sessionId,
                    GetSessionByIdHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var session = await handler.Handle(new GetSessionByIdQuery(sessionId), cancellationToken);
                    if (session is null)
                    {
                        return Results.NotFound();
                    }
                    return Results.Ok(session);
                });

            endpoints.MapGet("/api/sessions",
                async (
                    GetSessionsHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var sessions = await handler.Handle(new GetSessionsQuery(), cancellationToken);
                    return Results.Ok(sessions);
                });
            endpoints.MapPost("api/sessions/{sessionId:guid}/plan",
                async (
                    Guid sessionId,
                    PlanSessionHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    var command = new PlanSessionCommand(sessionId);

                    var tasks = await handler.Handle(command, cancellationToken);

                    return Results.Ok(tasks);
                });

            return endpoints;
        }
    }
}