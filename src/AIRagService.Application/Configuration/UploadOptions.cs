namespace AIRagService.Application.Configuration;

public class UploadOptions
{
    public const string SectionName = "Upload";

    public long MaxFileSizeBytes { get; set; } = 20_971_520;

    public int MaxChunksPerDocument { get; set; } = 5000;

    public int MaxQuestionLength { get; set; } = 2000;

    public int MaxTopK { get; set; } = 20;
}
