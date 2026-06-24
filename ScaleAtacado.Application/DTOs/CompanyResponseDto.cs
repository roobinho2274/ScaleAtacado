namespace ScaleAtacado.Application.DTOs;

public record CompanyResponseDto(
    Guid    Id,
    string  Name,
    string  CNPJ,
    string? Address,
    string? Phone,
    bool    IsActive,
    string? LogoBase64
);
