using AIRagService.Application.Interfaces;
using AIRagService.Application.Configuration;
using Microsoft.Extensions.Options;

namespace AIRagService.Infrastructure.Chunking;

public class TextChunker(IOptions<RagOptions> options) : ITextChunker
{
    public IReadOnlyList<TextChunkResult> Chunk(string text, int pageNumber)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        text = text.Trim();
        var chunkSize = Math.Max(1, options.Value.ChunkSize);
        var overlap = Math.Clamp(options.Value.ChunkOverlap, 0, chunkSize - 1);
        var step = chunkSize - overlap;

        if (text.Length <= chunkSize)
        {
            return
            [
                new TextChunkResult(text, pageNumber, 0, text.Length)
            ];
        }

        var results = new List<TextChunkResult>();

        for (var start = 0; start < text.Length; start += step)
        {
            var end = Math.Min(start + chunkSize, text.Length);
            var splitEnd = end;

            if (end < text.Length)
                splitEnd = FindSplitIndex(text, start, end);

            var content = text[start..splitEnd].Trim();
            if (content.Length > 0)
                results.Add(new TextChunkResult(content, pageNumber, start, splitEnd));

            if (splitEnd >= text.Length)
                break;
        }

        return results;
    }

    private static int FindSplitIndex(string text, int start, int hardEnd)
    {
        var minLength = Math.Max(1, (hardEnd - start) / 4);

        for (var i = hardEnd; i > start + minLength; i--)
        {
            if (text[i - 1] is '.' or '!' or '?' && (i == text.Length || char.IsWhiteSpace(text[i])))
                return i;
        }

        for (var i = hardEnd; i > start + minLength; i--)
        {
            if (char.IsWhiteSpace(text[i - 1]))
                return i;
        }

        return hardEnd;
    }
}
