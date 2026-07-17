using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ScaleAtacado.Api.Extensions;
using ScaleAtacado.Api.Hubs;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Services;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly OrderAppService _service;
    private readonly IHubContext<PrintHub> _printHub;

    public OrderController(OrderAppService service, IHubContext<PrintHub> printHub)
    {
        _service = service;
        _printHub = printHub;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DeliveryStatus? deliveryStatus = null,
        [FromQuery] FinancialStatus? financialStatus = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _service.GetAllAsync(
            User.GetCompanyId(), page, pageSize,
            customerId, deliveryStatus, financialStatus, from, to);
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
        var result = await _service.CreateAsync(dto, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPost("{id:guid}/finalize")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Finalize(Guid id)
    {
        var result = await _service.FinalizeAsync(id, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:guid}/printjob")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePrintJob(Guid id, [FromBody] CreatePrintJobRequestDto dto)
    {
        var result = await _service.CreatePrintJobAsync(id, User.GetCompanyId(), User.GetUserId(), User.GetFullName(), dto.Copies);
        if (!result.Success) return BadRequest(result);

        var signalRJob = new PendingPrintJobDto(result.Data, id, 0, dto.Copies);
        await _printHub.Clients.Group("PrintAgents")
            .SendAsync("NewPrintJob", signalRJob);

        return Ok(ApiResponse.Ok());
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unlock(Guid id)
    {
        var result = await _service.UnlockAsync(id, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/delivery")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDeliveryStatus(Guid id, [FromBody] UpdateDeliveryStatusDto dto)
    {
        var result = await _service.UpdateDeliveryStatusAsync(id, dto.DeliveryStatus, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/payment")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePaymentMethod(Guid id, [FromBody] UpdateOrderPaymentMethodDto dto)
    {
        var result = await _service.UpdatePaymentMethodAsync(id, dto, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/discount")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateDiscount(Guid id, [FromBody] UpdateDiscountDto dto)
    {
        var result = await _service.UpdateDiscountAsync(id, dto.DiscountAmount, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/items")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateItems(Guid id, [FromBody] UpdateOrderItemsDto dto)
    {
        var result = await _service.UpdateItemsAsync(id, dto, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id:guid}/financial")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFinancialStatus(Guid id, [FromBody] UpdateFinancialStatusDto dto)
    {
        var result = await _service.UpdateFinancialStatusAsync(id, dto.FinancialStatus, User.GetCompanyId(), User.GetUserId(), User.GetFullName());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
