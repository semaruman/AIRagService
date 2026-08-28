namespace AIRagService.Application.Interfaces;

public record TextChunkResult(
    string Content,
    int PageNumber,
    int CharacterStart,
    int CharacterEnd);

public interface ITextChunker
{
    IReadOnlyList<TextChunkResult> Chunk(string text, int pageNumber);
}
