using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ScaleAtacado.Api.Hubs;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Application.Services;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrintJobController : ControllerBase
{
    private readonly IPrintJobRepository _repository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IHubContext<PrintHub> _hub;
    private readonly IConfiguration _configuration;

    public PrintJobController(
        IPrintJobRepository repository,
        IOrderRepository orderRepository,
        ICompanyRepository companyRepository,
        IHubContext<PrintHub> hub,
        IConfiguration configuration)
    {
        _repository = repository;
        _orderRepository = orderRepository;
        _companyRepository = companyRepository;
        _hub = hub;
        _configuration = configuration;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        if (!IsValidAgent())
            return Unauthorized(ApiResponse.Fail("AgentKey inválida."));

        var pending = await _repository.GetPendingAsync();
        var failed = await _repository.GetFailedAsync();

        var all = pending.Concat(failed).Select(j => new PendingPrintJobDto(j.Id, j.OrderId, j.TryCount, j.Copies));
        return Ok(ApiResponse<IEnumerable<PendingPrintJobDto>>.Ok(all));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePrintJobStatusDto dto)
    {
        if (!IsValidAgent())
            return Unauthorized(ApiResponse.Fail("AgentKey inválida."));

        var job = await _repository.GetByIdAsync(id);
        if (job == null)
            return NotFound(ApiResponse.Fail("Job de impressão não encontrado."));

        job.Status = dto.Status;
        job.ErrorMessage = dto.ErrorMessage;
        job.TryCount += dto.Status == PrintStatus.Canceled ? 1 : 0;
        job.OnProcessed = dto.Status == PrintStatus.Printed ? DateTime.UtcNow : null;

        await _repository.UpdateAsync(job);
        await _repository.SaveChangesAsync();

        await _hub.Clients.Group("Clients")
            .SendAsync("PrintJobStatusChanged", new { job.Id, job.OrderId, Status = dto.Status.ToString() });

        return Ok(ApiResponse.Ok());
    }

    [HttpGet("{jobId:guid}/order")]
    public async Task<IActionResult> GetOrderForAgent(Guid jobId)
    {
        if (!IsValidAgent())
            return Unauthorized(ApiResponse.Fail("AgentKey inválida."));

        var job = await _repository.GetByIdAsync(jobId);
        if (job == null)
            return NotFound(ApiResponse.Fail("Job não encontrado."));

        var order = await _orderRepository.GetByIdAsync(job.OrderId, Guid.Empty);
        if (order == null)
            return NotFound(ApiResponse.Fail("Pedido não encontrado."));

        var company = await _companyRepository.GetByIdAsync(order.CompanyId);

        var dto = new OrderResponseDto(
            order.Id, order.OrderNumber, order.CompanyId,
            order.CustomerId, order.Customer.LegalName,
            order.IsInstallment,
            order.SurchargePercentage,
            order.PaymentMethods.Select(opm => new OrderPaymentMethodDto(opm.PaymentMethodId, opm.PaymentMethod?.Name ?? string.Empty, opm.Amount)).ToList(),
            order.UserId, order.OrderDate,
            order.AmountTotal, order.DiscountAmount, order.AmountWithSurchargeTotal,
            order.DeliveryStatus, order.FinancialStatus, order.IsLocked,
            order.Items.Select(i => new OrderItemResponseDto(
                i.Id, i.ProductId, i.ProductName, i.ProductCode,
                i.Quantity, i.UnitPrice, i.TotalPrice)).ToList(),
            CompanyName:        company?.Name,
            CompanyCNPJ:        company?.CNPJ,
            CompanyAddress:     company?.Address,
            CompanyLogoBase64:  company?.LogoBase64,
            Version:            order.Version
        );

        return Ok(ApiResponse<OrderResponseDto>.Ok(dto));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var pending = await _repository.GetPendingAsync();
        var dtos = pending.Select(j => new PendingPrintJobDto(j.Id, j.OrderId, j.TryCount, j.Copies));
        return Ok(ApiResponse<IEnumerable<PendingPrintJobDto>>.Ok(dtos));
    }

    private bool IsValidAgent()
    {
        var key = Request.Headers["X-Agent-Key"].ToString();
        return key == _configuration["PrintAgent:AgentKey"];
    }
}
