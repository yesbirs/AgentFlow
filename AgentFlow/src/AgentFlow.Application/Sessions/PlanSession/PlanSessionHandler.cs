using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Planning;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Sessions.PlanSession;

public sealed class PlanSessionHandler
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IPlanner _planner;
    private readonly PlanValidator _validator;
    private readonly PlanApplicationService _planApplicationService;

    public PlanSessionHandler(
        ISessionRepository sessionRepository,
        IPlanner planner,
        PlanValidator validator,
        PlanApplicationService planApplicationService)
    {
        _sessionRepository = sessionRepository;
        _planner = planner;
        _validator = validator;
        _planApplicationService = planApplicationService;
    }

    public async Task<IReadOnlyList<AgentTask>> Handle(
        PlanSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var session = await _sessionRepository.GetSessionByIdAsync(
            command.SessionId,
            cancellationToken);

        if (session is null)
        {
            throw new InvalidOperationException(
                $"Session '{command.SessionId}' does not exist.");
        }

        var plan = await _planner.CreatePlanAsync(
            session.Prompt,
            cancellationToken);

        _validator.Validate(plan);

        return await _planApplicationService.ApplyAsync(
            session.Id,
            plan,
            cancellationToken);
    }
}