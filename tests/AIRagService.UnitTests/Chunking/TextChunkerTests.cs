using AIRagService.Application.Configuration;
using AIRagService.Infrastructure.Chunking;
using Microsoft.Extensions.Options;

namespace AIRagService.UnitTests.Chunking;

public class TextChunkerTests
{
    private readonly TextChunker _chunker = new(Options.Create(new RagOptions
    {
        ChunkSize = 800,
        ChunkOverlap = 120
    }));

    [Fact]
    public void Chunk_LongText_ProducesChunksNearTargetSize()
    {
        var text = string.Join(' ', Enumerable.Repeat("word", 500));

        var chunks = _chunker.Chunk(text, pageNumber: 1);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks.Take(chunks.Count - 1), chunk =>
            Assert.InRange(chunk.Content.Length, 200, 800));
    }

    [Fact]
    public void Chunk_LongText_OverlapPreservesSharedContent()
    {
        const int chunkSize = 800;
        const int overlap = 120;
        var text = new string('a', chunkSize * 3) + " unique-boundary-marker";

        var chunks = _chunker.Chunk(text, pageNumber: 1);

        Assert.True(chunks.Count >= 2);

        var firstEnd = chunks[0].CharacterEnd;
        var secondStart = chunks[1].CharacterStart;
        var sharedLength = firstEnd - secondStart;

        Assert.True(sharedLength >= overlap / 2,
            $"Expected overlap between chunks, but shared length was {sharedLength}.");
        Assert.Equal(
            text.AsSpan(secondStart, sharedLength),
            text.AsSpan(secondStart, sharedLength));
    }

    [Fact]
    public void Chunk_Text_DoesNotProduceEmptyChunks()
    {
        var text = string.Join(' ', Enumerable.Repeat("paragraph", 300));

        var chunks = _chunker.Chunk(text, pageNumber: 2);

        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.False(string.IsNullOrWhiteSpace(chunk.Content)));
        Assert.All(chunks, chunk => Assert.Equal(2, chunk.PageNumber));
    }

    [Fact]
    public void Chunk_EmptyText_ReturnsNoChunks()
    {
        var chunks = _chunker.Chunk("   ", pageNumber: 1);

        Assert.Empty(chunks);
    }
}
