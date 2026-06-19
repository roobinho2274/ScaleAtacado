namespace ScaleAtacado.Application.DTOs;

public record CustomerResponseDto(
    Guid Id,
    Guid CompanyId,
    string LegalName,
    string TaxId,
    string Address,
    string Phone,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt
);
