using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleAtacado.Api.Extensions;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Services;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly CompanyAppService _service;

    public CompanyController(CompanyAppService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CompanyResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAsync(User.GetCompanyId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateCompanyDto dto)
    {
        var result = await _service.UpdateAsync(User.GetCompanyId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("logo")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLogo([FromBody] UpdateCompanyLogoDto dto)
    {
        var result = await _service.UpdateLogoAsync(User.GetCompanyId(), dto.LogoBase64);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
