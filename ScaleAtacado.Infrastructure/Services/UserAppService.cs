using Microsoft.AspNetCore.Identity;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Infrastructure.Identity;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Infrastructure.Services;

public class UserAppService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAppService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<int> GenerateUniqueCodeAsync()
    {
        var rng = Random.Shared;
        for (int attempt = 0; attempt < 500; attempt++)
        {
            var code = rng.Next(1000, 10000);
            if (!_userManager.Users.Any(u => u.UserCode == code))
                return code;
        }
        throw new InvalidOperationException("Não há códigos de 4 dígitos disponíveis.");
    }

    public async Task<ApiResponse<UserResponseDto>> CreateAsync(CreateUserDto dto, Guid companyId)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return ApiResponse<UserResponseDto>.Fail("Já existe um usuário com este e-mail.");

        var code = await GenerateUniqueCodeAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            CompanyId = companyId,
            Profile = dto.Profile,
            IsActive = true,
            UserCode = code,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<UserResponseDto>.Fail(errors);
        }

        return ApiResponse<UserResponseDto>.Ok(ToDto(user));
    }

    public async Task<ApiResponse<UserResponseDto>> UpdateAsync(Guid id, UpdateUserDto dto, Guid companyId)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.CompanyId != companyId)
            return ApiResponse<UserResponseDto>.Fail("Usuário não encontrado.");

        user.FullName = dto.FullName;
        user.Profile = dto.Profile;
        user.IsActive = dto.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<UserResponseDto>.Fail(errors);
        }

        return ApiResponse<UserResponseDto>.Ok(ToDto(user));
    }

    public async Task<ApiResponse<UserResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.CompanyId != companyId)
            return ApiResponse<UserResponseDto>.Fail("Usuário não encontrado.");

        return ApiResponse<UserResponseDto>.Ok(ToDto(user));
    }

    public ApiResponse<IEnumerable<UserResponseDto>> GetAll(Guid companyId)
    {
        var users = _userManager.Users
            .Where(u => u.CompanyId == companyId)
            .OrderBy(u => u.FullName)
            .ToList();

        return ApiResponse<IEnumerable<UserResponseDto>>.Ok(users.Select(ToDto));
    }

    public async Task<ApiResponse> ChangePasswordAsync(Guid id, string newPassword, Guid companyId)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.CompanyId != companyId)
            return ApiResponse.Fail("Usuário não encontrado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse.Fail(errors);
        }

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse<int>> RegenerateCodeAsync(Guid id, Guid companyId)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.CompanyId != companyId)
            return ApiResponse<int>.Fail("Usuário não encontrado.");

        var newCode = await GenerateUniqueCodeAsync();
        user.UserCode = newCode;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApiResponse<int>.Fail(errors);
        }

        return ApiResponse<int>.Ok(newCode);
    }

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.CompanyId != companyId)
            return ApiResponse.Fail("Usuário não encontrado.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> ReactivateAsync(Guid id, Guid companyId)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.CompanyId != companyId)
            return ApiResponse.Fail("Usuário não encontrado.");

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        return ApiResponse.Ok();
    }

    private static UserResponseDto ToDto(ApplicationUser u) => new(
        u.Id, u.CompanyId, u.FullName, u.Email!, u.Profile, u.IsActive, u.CreatedAt, u.UserCode
    );
}
