using AgentFlow.Application.Abstractions.LLM;
using AgentFlow.Application.Sessions.Services;

namespace AgentFlow.Infrastructure.Planning;

public sealed class OllamaSessionResultGenerator : ISessionResultGenerator
{
    private readonly ILlmClient _llmClient;

    public OllamaSessionResultGenerator(ILlmClient llmClient)
    {
        _llmClient = llmClient;
    }

    public async Task<string> GenerateAsync(
        string sessionObjective,
        IReadOnlyList<SessionTaskOutput> taskOutputs,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionObjective))
            throw new ArgumentException("Session objective cannot be empty.");

        if (taskOutputs is null || taskOutputs.Count == 0)
            throw new ArgumentException("At least one task output is required.");

        const string systemPrompt = """
            You are the final response generator for AgentFlow,
            an AI agent workflow orchestration engine.

            Your responsibility is to combine outputs from multiple
            worker agents into one comprehensive response.

            Follow these rules:

            1. Address the original session objective.
            2. Combine related information into logical sections.
            3. Remove duplicated information.
            4. Preserve important technical details.
            5. Identify conrified work was completed.
            7. If information is missing, explicittradictions between worker outputs.
            6. Never invent facts or claim unvely mention it.
            8. Correct obvious formatting problems without changing meaning.
            9. Do not blindly trust worker outputs.
            10. Produce a clear, structured and readable final response.

            Return only the final response.
            """;

        var taskSections = string.Join(
            "\n\n",
            taskOutputs.Select((task, index) => $"""
                TASK {index + 1}

                Title: {task.TaskTitle}

                Description: {task.TaskDescription}

                Worker Output:
                {task.Content}
                """));

        var userPrompt = $"""
            ORIGINAL SESSION OBJECTIVE:

            {sessionObjective}

            INDIVIDUAL WORKER OUTPUTS:

            {taskSections}

            Now synthesize these outputs into one comprehensive
            response that fulfills the original objective.
            """;

        var result = await _llmClient.GenerateAsync(
            systemPrompt,
            userPrompt,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                "The session result generator returned an empty response.");
        }

        return result.Trim();
    }
}