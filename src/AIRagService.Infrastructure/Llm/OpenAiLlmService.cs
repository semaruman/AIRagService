using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIRagService.Application.Configuration;
using AIRagService.Application.DTOs;
using AIRagService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AIRagService.Infrastructure.Llm;

public class OpenAiLlmService : ILlmService
{
    private const string SystemPrompt =
        "You are a helpful assistant that answers questions using only the provided context. " +
        "If the answer is not contained in the context, respond that you do not have enough information. " +
        "Do not use outside knowledge.";

    private readonly HttpClient _httpClient;
    private readonly LlmOptions _options;

    public OpenAiLlmService(HttpClient httpClient, IOptions<LlmOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<string> GenerateAnswerAsync(
        string question,
        IReadOnlyList<ContextChunk> context,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("OpenAI LLM is not configured. Set Llm:ApiKey.");

        var contextText = string.Join(
            Environment.NewLine + Environment.NewLine,
            context.Select((chunk, index) =>
                $"[{index + 1}] {chunk.FileName} (page {chunk.PageNumber?.ToString() ?? "n/a"}): {chunk.Content}"));

        var request = new ChatCompletionRequest
        {
            Model = _options.Model,
            Messages =
            [
                new ChatMessage("system", SystemPrompt),
                new ChatMessage(
                    "user",
                    $"Context:{Environment.NewLine}{contextText}{Environment.NewLine}{Environment.NewLine}" +
                    $"Question: {question}")
            ]
        };

        using var response = await _httpClient.PostAsJsonAsync("chat/completions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("OpenAI chat completion response was empty.");

        return payload.Choices.FirstOrDefault()?.Message.Content?.Trim()
            ?? string.Empty;
    }

    private sealed class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];
    }

    private sealed class ChatMessage(string role, string content)
    {
        [JsonPropertyName("role")]
        public string Role { get; } = role;

        [JsonPropertyName("content")]
        public string Content { get; } = content;
    }

    private sealed class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice> Choices { get; set; } = [];
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatChoiceMessage Message { get; set; } = new();
    }

    private sealed class ChatChoiceMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
