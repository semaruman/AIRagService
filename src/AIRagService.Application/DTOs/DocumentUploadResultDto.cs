namespace AIRagService.Application.DTOs;

public class DocumentUploadResultDto
{
    public required DocumentDto Document { get; set; }

    public bool AlreadyExists { get; set; }
}
