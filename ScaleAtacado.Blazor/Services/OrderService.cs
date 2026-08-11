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
        OrderStatus? orderStatus = null, FinancialStatus? financialStatus = null,
        DateTime? from = null, DateTime? to = null)
    {
        var q = $"api/order?page={page}&pageSize={pageSize}";
        if (orderStatus.HasValue)     q += $"&orderStatus={(int)orderStatus.Value}";
        if (financialStatus.HasValue) q += $"&financialStatus={(int)financialStatus.Value}";
        if (from.HasValue) q += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue)   q += $"&to={to.Value:yyyy-MM-dd}";
        return _api.GetAsync<PagedResult<OrderListItemDto>>(q);
    }

    public Task<ApiResponse<OrderResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<OrderResponseDto>($"api/order/{id}");

    public Task<ApiResponse<OrderResponseDto>?> CreateAsync(CreateOrderDto dto)
        => _api.PostAsync<OrderResponseDto>("api/order", dto);

    public Task<ApiResponse?> FinalizeAsync(Guid id)
        => _api.PostAsync($"api/order/{id}/finalize");

    public Task<ApiResponse?> UnlockAsync(Guid id)
        => _api.PostAsync($"api/order/{id}/unlock");

    public Task<ApiResponse?> CreatePrintJobAsync(Guid id, int copies = 1)
        => _api.PostAsync($"api/order/{id}/printjob", new { Copies = copies });

    public Task<ApiResponse?> UpdatePaymentMethodAsync(Guid id, UpdateOrderPaymentMethodDto dto)
        => _api.PatchAsync($"api/order/{id}/payment", dto);

    public Task<ApiResponse?> UpdateDiscountAsync(Guid id, decimal discountAmount)
        => _api.PatchAsync($"api/order/{id}/discount", new UpdateDiscountDto(discountAmount));

    public Task<ApiResponse?> UpdateItemsAsync(Guid id, UpdateOrderItemsDto dto)
        => _api.PatchAsync($"api/order/{id}/items", dto);

    public Task<ApiResponse?> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        => _api.PatchAsync($"api/order/{id}/status", new UpdateOrderStatusDto(status));

    public Task<ApiResponse?> UpdateFinancialStatusAsync(Guid id, FinancialStatus status)
        => _api.PatchAsync($"api/order/{id}/financial", new UpdateFinancialStatusDto(status));

    public Task<ApiResponse?> UpdateNotesAsync(Guid id, string? notes)
        => _api.PatchAsync($"api/order/{id}/notes", new UpdateOrderNotesDto(notes));
}
