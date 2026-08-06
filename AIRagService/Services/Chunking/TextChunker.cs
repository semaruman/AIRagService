namespace AIRagService.Services.Chunking;

public class TextChunker : ITextChunker
{
    public IReadOnlyList<string> Chunk(
        string text,
        int minLength = 50,
        int targetLength = 80,
        int maxLength = 100)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        text = text.Trim();
        if (text.Length <= maxLength)
            return [text];

        var chunks = new List<string>();
        var start = 0;

        while (start < text.Length)
        {
            var remaining = text.Length - start;
            if (remaining <= maxLength)
            {
                var tail = text[start..].Trim();
                if (tail.Length > 0)
                    chunks.Add(tail);
                break;
            }

            var preferredEnd = Math.Min(start + targetLength, text.Length);
            var hardEnd = Math.Min(start + maxLength, text.Length);

            var splitAt = FindSplitIndex(text, start, preferredEnd, hardEnd, minLength);
            var chunk = text[start..splitAt].Trim();
            if (chunk.Length > 0)
                chunks.Add(chunk);

            start = splitAt;
            while (start < text.Length && char.IsWhiteSpace(text[start]))
                start++;
        }

        return chunks;
    }

    private static int FindSplitIndex(string text, int start, int preferredEnd, int hardEnd, int minLength)
    {
        // Prefer sentence boundary near the target.
        for (var i = preferredEnd; i > start + minLength; i--)
        {
            if (text[i - 1] is '.' or '!' or '?' && (i == text.Length || char.IsWhiteSpace(text[i])))
                return i;
        }

        // Prefer word boundary before hard max.
        for (var i = hardEnd; i > start + minLength; i--)
        {
            if (char.IsWhiteSpace(text[i - 1]))
                return i - 1;
        }

        // Fall back: cut at max (may split a word).
        return hardEnd;
    }
}
