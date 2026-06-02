namespace ScaleAtacado.Application.DTOs;

public record CategoryResponseDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    string? Description
);
