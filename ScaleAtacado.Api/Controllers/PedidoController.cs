using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScaleAtacado.Api.Extensions;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Services;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidoController : ControllerBase
{
    private readonly OrderAppService _service;

    public PedidoController(OrderAppService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? clienteId = null,
        [FromQuery] DeliveryStatus? deliveryStatus = null,
        [FromQuery] FinancialStatus? financialStatus = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _service.GetAllAsync(
            User.GetCompanyId(), page, pageSize,
            clienteId, deliveryStatus, financialStatus, from, to);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id, User.GetCompanyId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var result = await _service.CreateAsync(dto, User.GetCompanyId(), User.GetUserId());
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Finalizar(Guid id)
    {
        var result = await _service.FinalizarAsync(id, User.GetCompanyId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/desbloquear")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desbloquear(Guid id)
    {
        var result = await _service.DesbloquearAsync(id, User.GetCompanyId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/entrega")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDeliveryStatus(Guid id, [FromBody] UpdateDeliveryStatusDto dto)
    {
        var result = await _service.UpdateDeliveryStatusAsync(id, dto.DeliveryStatus, User.GetCompanyId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/financeiro")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFinancialStatus(Guid id, [FromBody] UpdateFinancialStatusDto dto)
    {
        var result = await _service.UpdateFinancialStatusAsync(id, dto.FinancialStatus, User.GetCompanyId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
