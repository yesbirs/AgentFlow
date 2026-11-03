using System.Text;
using AgentFlow.Application.Abstractions.LLM;
using AgentFlow.Application.Abstractions.Execution;
using AgentFlow.Application.Abstractions.Persistence;
using AgentFlow.Application.Execution.Context;
using AgentFlow.Domain.Entities;

namespace AgentFlow.Application.Execution;

public sealed class LlmTaskExecutor : ITaskExecutor
{
    private readonly ILlmClient _llmClient;
    private readonly ITaskContextProvider _contextProvider;
    private readonly ITaskResultRepository _resultRepository;

    public LlmTaskExecutor(
        ILlmClient llmClient,
        ITaskContextProvider contextProvider,
        ITaskResultRepository resultRepository)
    {
        _llmClient = llmClient;
        _contextProvider = contextProvider;
        _resultRepository = resultRepository;
    }

    public async Task ExecuteAsync(
        AgentTask task,
        CancellationToken cancellationToken = default)
    {
        // 1. Build execution context
        var context = await _contextProvider.BuildAsync(
            task,
            cancellationToken);

        // 2. Build prompts
        const string systemPrompt = """
            You are an intelligent AI task execution assistant.

            Your responsibility is to complete the assigned task
            using the provided session objective and dependency results.

            Follow these rules:
            - Focus on the current task.
            - Use previous task results when relevant.
            - Do not invent information.
            - Provide clear, structured and useful output.
            """;

        var userPrompt = BuildUserPrompt(context);

        // 3. Call Ollama
        var result = await _llmClient.GenerateAsync(
            systemPrompt,
            userPrompt,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                "LLM returned an empty result.");
        }

        // 4. Persist generated result
        var taskResult = new TaskResult(task.Id, result);

        await _resultRepository.AddAsync(
            taskResult,
            cancellationToken);

        Console.WriteLine(
            $"Task '{task.Title}' generated a result.");
    }

    private static string BuildUserPrompt(TaskContext context)
    {
        var builder = new StringBuilder();

        builder.AppendLine("SESSION OBJECTIVE:");
        builder.AppendLine(context.SessionObjective);

        builder.AppendLine();
        builder.AppendLine("CURRENT TASK:");
        builder.AppendLine(context.TaskTitle);

        builder.AppendLine();
        builder.AppendLine("TASK DESCRIPTION:");
        builder.AppendLine(context.TaskDescription);

        builder.AppendLine();
        builder.AppendLine("COMPLETED DEPENDENCY RESULTS:");

        foreach (var dependency in context.Dependencies)
        {
            builder.AppendLine();
            builder.AppendLine($"Dependency: {dependency.TaskTitle}");
            builder.AppendLine(
                $"Description: {dependency.TaskDescription}");
            builder.AppendLine("Result:");
            builder.AppendLine(dependency.Result);
        }

        return builder.ToString();
    }
}