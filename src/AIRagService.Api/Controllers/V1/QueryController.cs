using AIRagService.Application.DTOs;
using AIRagService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIRagService.Api.Controllers.V1;

[ApiController]
[Route("api/v1/query")]
[Produces("application/json")]
public class QueryController(IRagQueryService ragQueryService) : ControllerBase
{
  [HttpPost]
  [EnableRateLimiting("query")]
  [ProducesResponseType(typeof(QueryResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<QueryResponseDto>> Query(
      [FromBody] QueryRequestDto request,
      CancellationToken cancellationToken)
  {
      var response = await ragQueryService.QueryAsync(request, cancellationToken);
      return Ok(response);
  }
}
