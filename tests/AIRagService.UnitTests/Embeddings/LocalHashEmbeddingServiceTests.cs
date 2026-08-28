using AIRagService.Application.Configuration;
using AIRagService.Infrastructure.Embeddings;
using Microsoft.Extensions.Options;

namespace AIRagService.UnitTests.Embeddings;

public class LocalHashEmbeddingServiceTests
{
    private readonly LocalHashEmbeddingService _service = new(Options.Create(new EmbeddingOptions
    {
        Provider = "Local",
        Dimensions = 1536
    }));

    [Fact]
    public async Task GenerateEmbeddings_Returns1536Dimensions()
    {
        var embeddings = await _service.GenerateEmbeddingsAsync(["hello world"]);

        Assert.Single(embeddings);
        Assert.Equal(1536, embeddings[0].Length);
    }

    [Fact]
    public async Task GenerateEmbeddings_IsDeterministic()
    {
        const string text = "deterministic embedding input";

        var first = await _service.GenerateEmbeddingsAsync([text]);
        var second = await _service.GenerateEmbeddingsAsync([text]);

        Assert.Equal(first[0], second[0]);
    }

    [Fact]
    public async Task GenerateEmbeddings_ProducesNormalizedVectors()
    {
        var embeddings = await _service.GenerateEmbeddingsAsync(["normalization check"]);
        var vector = embeddings[0];

        var magnitude = MathF.Sqrt(vector.Sum(v => v * v));

        Assert.InRange(magnitude, 0.99f, 1.01f);
    }
}
