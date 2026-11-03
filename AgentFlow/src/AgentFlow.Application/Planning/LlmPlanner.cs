using System.Text.Json;
using AgentFlow.Application.Abstractions.LLM;
using AgentFlow.Application.Planning;

namespace AgentFlow.Infrastructure.Planning;

public sealed class LlmPlanner : IPlanner
{
    private readonly ILlmClient _llmClient;

    public LlmPlanner(ILlmClient llmClient)
    {
        _llmClient = llmClient;
    }

    public async Task<PlanningResult> CreatePlanAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        const string systemPrompt = """
            You are a planning assistant. Break the user's objective into a small set of
            concrete, actionable tasks that together achieve it.

            Rules:
            1. Every task must have a unique key.
            2. Tasks must be specific and actionable.
            3. Each task must have a clear title and description.
            4. parentKey MUST be null for top-level tasks. Only set it when the task is a
               subtask of a different task, and then use that parent's key, never the
               task's own key or title.
            5. Use dependencies only when one task genuinely requires
               another task to finish first.
            6. Do not create circular dependencies.
            7. Do not make every task depend on the previous task.
            8. Include enough tasks to accomplish the objective.
            9. Return only data matching the supplied JSON schema.
            """;

        var userPrompt = $"""
            Create an execution plan for this objective:

            {prompt}
            """;

        var schema = PlanningJsonSchema.GetSchema();

        var json = await _llmClient.GenerateStructuredAsync(
            systemPrompt,
            userPrompt,
            schema,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException(
                "The LLM returned an empty planning response.");
        }

        var result = JsonSerializer.Deserialize<PlanningResult>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return result ?? throw new InvalidOperationException(
            "The LLM returned an invalid planning response.");
    }
}