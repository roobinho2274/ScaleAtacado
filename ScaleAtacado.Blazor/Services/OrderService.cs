using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class OrderService
{
    private readonly ApiHttpClient _api;
    public OrderService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<PagedResult<OrderListItemDto>>?> GetAllAsync(
        int page = 1, int pageSize = 20,
        DeliveryStatus? deliveryStatus = null, FinancialStatus? financialStatus = null)
    {
        var q = $"api/pedido?page={page}&pageSize={pageSize}";
        if (deliveryStatus.HasValue) q += $"&deliveryStatus={(int)deliveryStatus.Value}";
        if (financialStatus.HasValue) q += $"&financialStatus={(int)financialStatus.Value}";
        return _api.GetAsync<PagedResult<OrderListItemDto>>(q);
    }

    public Task<ApiResponse<OrderResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<OrderResponseDto>($"api/pedido/{id}");

    public Task<ApiResponse<OrderResponseDto>?> CreateAsync(CreateOrderDto dto)
        => _api.PostAsync<OrderResponseDto>("api/pedido", dto);

    public Task<ApiResponse?> FinalizarAsync(Guid id)
        => _api.PostAsync($"api/pedido/{id}/finalizar");

    public Task<ApiResponse?> DesbloquearAsync(Guid id)
        => _api.PostAsync($"api/pedido/{id}/desbloquear");

    public Task<ApiResponse?> UpdateDiscountAsync(Guid id, decimal discountAmount)
        => _api.PatchAsync($"api/pedido/{id}/desconto", new UpdateDiscountDto(discountAmount));

    public Task<ApiResponse?> UpdateDeliveryStatusAsync(Guid id, DeliveryStatus status)
        => _api.PatchAsync($"api/pedido/{id}/entrega", new UpdateDeliveryStatusDto(status));

    public Task<ApiResponse?> UpdateFinancialStatusAsync(Guid id, FinancialStatus status)
        => _api.PatchAsync($"api/pedido/{id}/financeiro", new UpdateFinancialStatusDto(status));
}
