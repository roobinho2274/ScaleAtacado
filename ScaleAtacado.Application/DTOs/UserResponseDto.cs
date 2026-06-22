using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record UserResponseDto(
    Guid Id,
    Guid CompanyId,
    string FullName,
    string Email,
    UserProfile Profile,
    bool IsActive,
    DateTime CreatedAt,
    int UserCode
);
