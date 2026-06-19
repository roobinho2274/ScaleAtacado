using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class CustomerService
{
    private readonly ApiHttpClient _api;
    public CustomerService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<CustomerResponseDto>>?> SearchAsync(string? term = null)
        => _api.GetAsync<IEnumerable<CustomerResponseDto>>(
            string.IsNullOrWhiteSpace(term) ? "api/customer" : $"api/customer?search={Uri.EscapeDataString(term)}");

    public Task<ApiResponse<CustomerResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<CustomerResponseDto>($"api/customer/{id}");

    public Task<ApiResponse<CustomerResponseDto>?> CreateAsync(CreateCustomerDto dto)
        => _api.PostAsync<CustomerResponseDto>("api/customer", dto);

    public Task<ApiResponse<CustomerResponseDto>?> UpdateAsync(Guid id, UpdateCustomerDto dto)
        => _api.PutAsync<CustomerResponseDto>($"api/customer/{id}", dto);

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/customer/{id}");
}
