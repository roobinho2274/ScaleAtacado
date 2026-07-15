using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class UserService
{
    private readonly ApiHttpClient _api;
    public UserService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<PagedResult<UserResponseDto>>?> GetAllAsync(
        int page = 1, int pageSize = 20, string? search = null)
    {
        var q = $"api/user?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) q += $"&search={Uri.EscapeDataString(search)}";
        return _api.GetAsync<PagedResult<UserResponseDto>>(q);
    }

    public Task<ApiResponse<UserResponseDto>?> CreateAsync(CreateUserDto dto)
        => _api.PostAsync<UserResponseDto>("api/user", dto);

    public Task<ApiResponse<UserResponseDto>?> UpdateAsync(Guid id, UpdateUserDto dto)
        => _api.PutAsync<UserResponseDto>($"api/user/{id}", dto);

    public Task<ApiResponse?> ChangePasswordAsync(Guid id, string newPassword)
        => _api.PatchAsync($"api/user/{id}/password", new ChangePasswordDto(newPassword));

    public Task<ApiResponse<int>?> RegenerateCodeAsync(Guid id)
        => _api.PatchAsync<int>($"api/user/{id}/code");

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/user/{id}");

    public Task<ApiResponse?> ReactivateAsync(Guid id)
        => _api.PatchAsync($"api/user/{id}/activate", new { });

    public Task<ApiResponse?> DeletePermanentAsync(Guid id)
        => _api.DeleteAsync($"api/user/{id}/permanent");
}
