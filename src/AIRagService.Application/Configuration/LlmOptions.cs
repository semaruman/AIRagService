namespace AIRagService.Application.Configuration;

public class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}
