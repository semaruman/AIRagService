using AIRagService.Application.Common;
using AIRagService.Application.DTOs;
using AIRagService.Application.Services;
using AIRagService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIRagService.Api.Controllers.V1;

[ApiController]
[Route("api/v1/documents")]
[Produces("application/json")]
public class DocumentsController(
    IDocumentService documentService,
    IDocumentIngestionService ingestionService,
    IDocumentIndexingService indexingService) : ControllerBase
{
  [HttpGet]
  [ProducesResponseType(typeof(PagedResult<DocumentDto>), StatusCodes.Status200OK)]
  public async Task<ActionResult<PagedResult<DocumentDto>>> GetDocuments(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      [FromQuery] DocumentStatus? status = null,
      CancellationToken cancellationToken = default)
  {
      pageSize = Math.Clamp(pageSize, 1, 100);
      var result = await documentService.GetPagedAsync(page, pageSize, status, cancellationToken);
      return Ok(result);
  }

  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<DocumentDetailDto>> GetDocument(Guid id, CancellationToken cancellationToken)
  {
      var document = await documentService.GetByIdAsync(id, cancellationToken);
      return Ok(document);
  }

  [HttpPost]
  [EnableRateLimiting("upload")]
  [Consumes("multipart/form-data")]
  [ProducesResponseType(typeof(DocumentUploadResultDto), StatusCodes.Status202Accepted)]
  [ProducesResponseType(typeof(DocumentUploadResultDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [RequestSizeLimit(20 * 1024 * 1024)]
  public async Task<ActionResult<DocumentUploadResultDto>> Upload(
      IFormFile? file,
      CancellationToken cancellationToken)
  {
      if (file is null || file.Length == 0)
          return BadRequest(new { title = "Validation failed", detail = "PDF file is required." });

      await using var stream = file.OpenReadStream();
      var result = await ingestionService.IngestAsync(stream, file.FileName, cancellationToken);

      if (result.AlreadyExists)
          return Ok(result);

      return Accepted(result);
  }

  [HttpDelete("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
      await documentService.DeleteAsync(id, cancellationToken);
      return NoContent();
  }

  [HttpPost("{id:guid}/index")]
  [ProducesResponseType(StatusCodes.Status202Accepted)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Index(Guid id, CancellationToken cancellationToken)
  {
      await indexingService.QueueIndexingAsync(id, cancellationToken);
      return Accepted(new { documentId = id, status = "Pending" });
  }
}
