using AgentFlow.Application.Sessions.CreateSession;
using AgentFlow.Application.Sessions.GetSessionById;
using AgentFlow.Application.Sessions.GetSessions;

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

            return endpoints;
        }
    }
}