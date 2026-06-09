using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class PaymentMethodService
{
    private readonly ApiHttpClient _api;
    public PaymentMethodService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<PaymentMethodResponseDto>>?> GetAllAsync(bool onlyActive = false)
        => _api.GetAsync<IEnumerable<PaymentMethodResponseDto>>($"api/paymentmethod?onlyActive={onlyActive}");

    public Task<ApiResponse<PaymentMethodResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<PaymentMethodResponseDto>($"api/paymentmethod/{id}");

    public Task<ApiResponse<PaymentMethodResponseDto>?> CreateAsync(CreatePaymentMethodDto dto)
        => _api.PostAsync<PaymentMethodResponseDto>("api/paymentmethod", dto);

    public Task<ApiResponse<PaymentMethodResponseDto>?> UpdateAsync(Guid id, UpdatePaymentMethodDto dto)
        => _api.PutAsync<PaymentMethodResponseDto>($"api/paymentmethod/{id}", dto);

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/paymentmethod/{id}");
}
