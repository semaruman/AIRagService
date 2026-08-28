using System.Security.Cryptography;
using AIRagService.Application.Configuration;
using AIRagService.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace AIRagService.Infrastructure.Embeddings;

public class LocalHashEmbeddingService(IOptions<EmbeddingOptions> options) : IEmbeddingService
{
    public Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<float[]> embeddings = texts.Select(GenerateEmbedding).ToArray();
        return Task.FromResult(embeddings);
    }

    private float[] GenerateEmbedding(string text)
    {
        var dimensions = Math.Max(1, options.Value.Dimensions);
        var embedding = new float[dimensions];
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text));

        for (var i = 0; i < dimensions; i++)
        {
            var byteIndex = i % hash.Length;
            embedding[i] = (hash[byteIndex] / 255f) * 2f - 1f;
        }

        Normalize(embedding);
        return embedding;
    }

    private static void Normalize(float[] vector)
    {
        var magnitude = 0f;
        foreach (var value in vector)
            magnitude += value * value;

        magnitude = MathF.Sqrt(magnitude);
        if (magnitude <= float.Epsilon)
            return;

        for (var i = 0; i < vector.Length; i++)
            vector[i] /= magnitude;
    }
}
