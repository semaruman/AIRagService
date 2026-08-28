namespace AIRagService.Application.Configuration;

public class EmbeddingOptions
{
    public const string SectionName = "Embedding";

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public int BatchSize { get; set; } = 64;

    public int Dimensions { get; set; } = 1536;

    public string BaseUrl { get; set; } = string.Empty;
}
