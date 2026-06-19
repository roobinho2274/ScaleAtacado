namespace ScaleAtacado.Application.DTOs;

public record CreateCustomerDto(
    string LegalName,
    string TaxId,
    string Address,
    string Phone,
    string? Notes
);
