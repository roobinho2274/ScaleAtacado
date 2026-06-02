using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record CreateUserDto(
    string FullName,
    string Email,
    string Password,
    UserProfile Profile
);
