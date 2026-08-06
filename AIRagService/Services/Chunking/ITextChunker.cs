namespace AIRagService.Services.Chunking;

public interface ITextChunker
{
    IReadOnlyList<string> Chunk(string text, int minLength = 50, int targetLength = 80, int maxLength = 100);
}
