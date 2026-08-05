using AIRagService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRagService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentChunksController(IDocumentChunkService documentChunkService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var chunks = await documentChunkService.GetAllAsync(cancellationToken);
        return Ok(chunks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var chunk = await documentChunkService.GetByIdAsync(id, cancellationToken);
        if (chunk is null)
            return NotFound();

        return Ok(chunk);
    }
}
