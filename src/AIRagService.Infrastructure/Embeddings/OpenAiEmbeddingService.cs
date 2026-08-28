using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIRagService.Application.Configuration;
using AIRagService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIRagService.Infrastructure.Embeddings;

public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly EmbeddingOptions _options;
    private readonly ILogger<OpenAiEmbeddingService> _logger;

    public OpenAiEmbeddingService(
        HttpClient httpClient,
        IOptions<EmbeddingOptions> options,
        ILogger<OpenAiEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0)
            return [];

        var request = new EmbeddingRequest
        {
            Model = _options.Model,
            Input = texts.ToArray(),
            Dimensions = _options.Dimensions
        };

        using var response = await _httpClient.PostAsJsonAsync("embeddings", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "OpenAI embedding request failed with status {StatusCode}: {Body}",
                response.StatusCode,
                body);

            throw new HttpRequestException(
                $"OpenAI embedding request failed with status {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        var payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("OpenAI embedding response was empty.");

        return payload.Data
            .OrderBy(item => item.Index)
            .Select(item => item.Embedding)
            .ToArray();
    }

    private sealed class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public string[] Input { get; set; } = [];

        [JsonPropertyName("dimensions")]
        public int Dimensions { get; set; }
    }

    private sealed class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; } = [];
    }

    private sealed class EmbeddingData
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }
}
