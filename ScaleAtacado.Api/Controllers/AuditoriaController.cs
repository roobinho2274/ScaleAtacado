using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleAtacado.Api.Extensions;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Services;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AuditLogController : ControllerBase
{
    private readonly AuditLogAppService _service;

    public AuditLogController(AuditLogAppService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AuditLogResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _service.GetByCompanyAsync(User.GetCompanyId(), from, to);
        return Ok(result);
    }
}
