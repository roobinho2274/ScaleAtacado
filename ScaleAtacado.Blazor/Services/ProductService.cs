using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class ProductService
{
    private readonly ApiHttpClient _api;
    public ProductService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<PagedResult<ProductResponseDto>>?> GetAllAsync(
        int page = 1, int pageSize = 20, string? search = null, bool? isActive = null)
    {
        var q = $"api/product?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) q += $"&search={Uri.EscapeDataString(search)}";
        if (isActive.HasValue) q += $"&isActive={isActive.Value}";
        return _api.GetAsync<PagedResult<ProductResponseDto>>(q);
    }

    public Task<ApiResponse<IEnumerable<ProductResponseDto>>?> GetAllActiveAsync()
        => _api.GetAsync<IEnumerable<ProductResponseDto>>("api/product?isActive=true&pageSize=1000");

    public Task<ApiResponse<ProductResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<ProductResponseDto>($"api/product/{id}");

    public Task<ApiResponse<ProductResponseDto>?> CreateAsync(CreateProductDto dto)
        => _api.PostAsync<ProductResponseDto>("api/product", dto);

    public Task<ApiResponse<ProductResponseDto>?> UpdateAsync(Guid id, UpdateProductDto dto)
        => _api.PutAsync<ProductResponseDto>($"api/product/{id}", dto);

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/product/{id}");
}
