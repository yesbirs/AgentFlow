using AgentFlow.Application.Abstractions.LLM;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace AgentFlow.Infrastructure.LLM;

public sealed class OllamaLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaLlmClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        _model = configuration["Ollama:Model"]
            ?? throw new InvalidOperationException(
                "Ollama model is not configured.");
    }

    public async Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _model,
            system = systemPrompt,
            prompt = userPrompt,
            stream = false
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    public async Task<string> GenerateStructuredAsync(
        string systemPrompt,
        string userPrompt,
        JsonElement jsonSchema,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _model,
            system = systemPrompt,
            prompt = userPrompt,
            format = jsonSchema,
            stream = false
        };

        return await SendRequestAsync(request, cancellationToken);
    }

    private async Task<string> SendRequestAsync(
        object request,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<OllamaResponse>(
                cancellationToken);

        if (result is null ||
            string.IsNullOrWhiteSpace(result.Response))
        {
            throw new InvalidOperationException(
                "Ollama returned an empty response.");
        }

        return result.Response;
    }

    private sealed class OllamaResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}