using AIRagService.Application.DTOs;
using AIRagService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRagService.Api.Controllers.V1;

[ApiController]
[Route("api/v1/stats")]
[Produces("application/json")]
public class StatsController(IDocumentService documentService) : ControllerBase
{
  [HttpGet]
  [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
  public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken cancellationToken)
  {
      return Ok(await documentService.GetStatsAsync(cancellationToken));
  }
}
