using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class CategoryService
{
    private readonly ApiHttpClient _api;
    public CategoryService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<CategoryResponseDto>>?> GetAllAsync()
        => _api.GetAsync<IEnumerable<CategoryResponseDto>>("api/category");

    public Task<ApiResponse<CategoryResponseDto>?> CreateAsync(CreateCategoryDto dto)
        => _api.PostAsync<CategoryResponseDto>("api/category", dto);

    public Task<ApiResponse<CategoryResponseDto>?> UpdateAsync(Guid id, UpdateCategoryDto dto)
        => _api.PutAsync<CategoryResponseDto>($"api/category/{id}", dto);

    public Task<ApiResponse?> DeleteAsync(Guid id)
        => _api.DeleteAsync($"api/category/{id}");
}
