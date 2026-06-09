using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class ClienteService
{
    private readonly ApiHttpClient _api;
    public ClienteService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<ClienteResponseDto>>?> SearchAsync(string? term = null)
        => _api.GetAsync<IEnumerable<ClienteResponseDto>>(
            string.IsNullOrWhiteSpace(term) ? "api/cliente" : $"api/cliente?search={Uri.EscapeDataString(term)}");

    public Task<ApiResponse<ClienteResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<ClienteResponseDto>($"api/cliente/{id}");

    public Task<ApiResponse<ClienteResponseDto>?> CreateAsync(CreateClienteDto dto)
        => _api.PostAsync<ClienteResponseDto>("api/cliente", dto);

    public Task<ApiResponse<ClienteResponseDto>?> UpdateAsync(Guid id, UpdateClienteDto dto)
        => _api.PutAsync<ClienteResponseDto>($"api/cliente/{id}", dto);

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/cliente/{id}");
}
