using AgentFlow.Application.Abstractions.LLM;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Sessions.Services;
using AgentFlow.Domain.Entities;
using AgentFlow.Domain.Enums;

namespace AgentFlow.Application.Sessions.Commands;

public sealed class FinalizeSessionHandler
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly ITaskResultRepository _taskResultRepository;
    private readonly ISessionResultRepository _sessionResultRepository;
    private readonly ISessionResultGenerator _resultGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizeSessionHandler(
        ISessionRepository sessionRepository,
        IAgentTaskRepository taskRepository,
        ITaskResultRepository taskResultRepository,
        ISessionResultRepository sessionResultRepository,
        ISessionResultGenerator resultGenerator,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _taskResultRepository = taskResultRepository;
        _sessionResultRepository = sessionResultRepository;
        _resultGenerator = resultGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<SessionResult> HandleAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        // 1. Check whether the session exists.
        var session = await _sessionRepository.GetSessionByIdAsync(
            sessionId,
            cancellationToken);

        if (session is null)
            throw new InvalidOperationException("Session not found.");

        // 2. Return existing result if already finalized.
        var existingResult =
            await _sessionResultRepository.GetBySessionIdAsync(
                sessionId,
                cancellationToken);

        if (existingResult is not null)
            return existingResult;

        // 3. Retrieve all session tasks.
        var tasks = await _taskRepository.GetBySessionIdAsync(
            sessionId,
            cancellationToken);

        if (tasks.Count == 0)
            throw new InvalidOperationException(
                "Cannot finalize a session without tasks.");

        // 4. Ensure every task has succeeded.
        var incompleteTasks = tasks
            .Where(task => task.State != TaskState.Succeeded)
            .ToList();

        if (incompleteTasks.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot finalize session. {incompleteTasks.Count} task(s) " +
                "have not succeeded.");
        }

        // 5. Collect the latest result from each task.
        var taskOutputs = new List<SessionTaskOutput>();

        foreach (var task in tasks)
        {
            var results = await _taskResultRepository.GetByTaskIdAsync(
                task.Id,
                cancellationToken);

            var latestResult = results.LastOrDefault();

            if (latestResult is null)
            {
                throw new InvalidOperationException(
                    $"Task '{task.Title}' succeeded but has no result.");
            }

            taskOutputs.Add(new SessionTaskOutput(
                task.Id,
                task.Title,
                task.Description,
                latestResult.Content));
        }

        // 6. Generate the consolidated response.
        var finalContent = await _resultGenerator.GenerateAsync(
            session.Prompt,
            taskOutputs,
            cancellationToken);

        // 7. Create and persist the session result.
        var sessionResult = new SessionResult(
            sessionId,
            finalContent);

        await _sessionResultRepository.AddAsync(
            sessionResult,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return sessionResult;
    }
}