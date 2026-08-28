namespace AIRagService.Application.Configuration;

public class ApiKeyAuthOptions
{
    public const string SectionName = "ApiKeyAuth";

    public string ApiKey { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}
