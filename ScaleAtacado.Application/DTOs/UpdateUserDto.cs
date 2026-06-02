using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record UpdateUserDto(
    string FullName,
    UserProfile Profile,
    bool IsActive
);
