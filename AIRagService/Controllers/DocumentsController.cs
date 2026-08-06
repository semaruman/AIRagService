using AIRagService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRagService.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController(IDocumentIngestionService documentIngestionService) : ControllerBase
{
    private const long MaxUploadBytes = 20 * 1024 * 1024;

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "PDF file is required." });

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only PDF files are supported." });
        }

        if (file.Length > MaxUploadBytes)
            return BadRequest(new { error = "File exceeds the 20 MB limit." });

        await using var stream = file.OpenReadStream();
        try
        {
            var result = await documentIngestionService.IngestPdfAsync(stream, file.FileName, cancellationToken);
            return Ok(new { source = result.Source, chunkCount = result.ChunkCount });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
