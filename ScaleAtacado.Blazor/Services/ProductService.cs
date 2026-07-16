using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class ProductService
{
    private readonly ApiHttpClient _api;
    public ProductService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<PagedResult<ProductResponseDto>>?> GetAllAsync(
        int page = 1, int pageSize = 20, string? search = null, bool? isActive = null, Guid? categoryId = null)
    {
        var q = $"api/product?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) q += $"&search={Uri.EscapeDataString(search)}";
        if (isActive.HasValue) q += $"&isActive={isActive.Value}";
        if (categoryId.HasValue) q += $"&categoryId={categoryId.Value}";
        return _api.GetAsync<PagedResult<ProductResponseDto>>(q);
    }

    public async Task<ApiResponse<IEnumerable<ProductResponseDto>>?> GetAllActiveAsync()
    {
        var resp = await _api.GetAsync<PagedResult<ProductResponseDto>>("api/product?isActive=true&pageSize=1000");
        if (resp?.Success == true && resp.Data != null)
            return ApiResponse<IEnumerable<ProductResponseDto>>.Ok(resp.Data.Items);
        return null;
    }

    public Task<ApiResponse<ProductResponseDto>?> GetByIdAsync(Guid id)
        => _api.GetAsync<ProductResponseDto>($"api/product/{id}");

    public Task<ApiResponse<ProductResponseDto>?> CreateAsync(CreateProductDto dto)
        => _api.PostAsync<ProductResponseDto>("api/product", dto);

    public Task<ApiResponse<ProductResponseDto>?> UpdateAsync(Guid id, UpdateProductDto dto)
        => _api.PutAsync<ProductResponseDto>($"api/product/{id}", dto);

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/product/{id}");

    public Task<ApiResponse?> DeletePermanentAsync(Guid id)
        => _api.DeleteAsync($"api/product/{id}/permanent");
}
