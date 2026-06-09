using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class UserService
{
    private readonly ApiHttpClient _api;
    public UserService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<UserResponseDto>>?> GetAllAsync()
        => _api.GetAsync<IEnumerable<UserResponseDto>>("api/user");

    public Task<ApiResponse<UserResponseDto>?> CreateAsync(CreateUserDto dto)
        => _api.PostAsync<UserResponseDto>("api/user", dto);

    public Task<ApiResponse<UserResponseDto>?> UpdateAsync(Guid id, UpdateUserDto dto)
        => _api.PutAsync<UserResponseDto>($"api/user/{id}", dto);

    public Task<ApiResponse?> DeactivateAsync(Guid id)
        => _api.DeleteAsync($"api/user/{id}");
}
