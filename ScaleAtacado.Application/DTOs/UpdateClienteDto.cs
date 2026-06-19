namespace ScaleAtacado.Application.DTOs;

public record UpdateCustomerDto(
    string LegalName,
    string TaxId,
    string Address,
    string Phone,
    string? Notes,
    bool IsActive
);
