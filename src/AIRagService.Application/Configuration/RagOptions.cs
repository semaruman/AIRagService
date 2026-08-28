namespace AIRagService.Application.Configuration;

public class RagOptions
{
    public const string SectionName = "Rag";

    public int ChunkSize { get; set; } = 800;

    public int ChunkOverlap { get; set; } = 120;

    public int TopK { get; set; } = 5;
}
