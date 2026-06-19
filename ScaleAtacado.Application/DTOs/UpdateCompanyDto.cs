namespace ScaleAtacado.Application.DTOs;

public record UpdateCompanyDto(
    string  Name,
    string  CNPJ,
    string? Address,
    string? Phone
);
