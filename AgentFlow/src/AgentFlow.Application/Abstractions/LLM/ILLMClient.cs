using System.Text.Json;

namespace AgentFlow.Application.Abstractions.LLM;

public interface ILlmClient
{
    Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);

    Task<string> GenerateStructuredAsync(string systemPrompt, string userPrompt, JsonElement jsonSchema, CancellationToken cancellationToken = default);
}